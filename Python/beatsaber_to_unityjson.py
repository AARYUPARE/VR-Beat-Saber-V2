"""
beatsaber_to_unityjson_smart.py

- Converts Beat Saber / Rasputin .dat "_notes" format into your Unity JSON:
  { "notes":[ { "time": spawn_time_seconds, "lane": -1/0/1, "color": "red"/"blue" }, ... ] }

- Handles:
  * variable BPM (bpm events)
  * travel offset (spawn earlier so notes reach player on hit)
  * clamps lanes to 3 lanes
  * limits total notes to [MIN_NOTES, MAX_NOTES]
  * biases selection toward high-energy sections of the audio
  * skips notes whose hit-time would be after the audio end
"""

import json
import random
import numpy as np
import librosa
import os

# -------------------- CONFIG --------------------
NOTE_SPEED = 10.0
SPAWN_Z = 10.0
PLAYER_Z = -10.0
TRAVEL_TIME = (SPAWN_Z - PLAYER_Z) / NOTE_SPEED  # seconds

MIN_NOTES = 150
MAX_NOTES = 720

input_file = r"C:\Users\Dell\Downloads\124 (Rasputin (Funk Overload) - jobas)\Hard.dat"
output_file = r"D:\Saksham\Unity\Projects\Gedit\VR-Beat-Saber-V2\Assets\Songs\Rasputin.json"
audio_file = r"C:\Users\Dell\Downloads\Rasputin.mp3"

# Small margin to allow notes close to song end (seconds)
END_MARGIN = 0.01

# audio analysis params
SR = None         # None -> use file's native sampling rate
HOP_LENGTH = 512

# -------------------- HELPERS --------------------
def find_bpm_events(data):
    # Try common keys for BPM events / BPM value
    if "_beatsPerMinute" in data:
        return [{"b": 0.0, "m": float(data["_beatsPerMinute"])}]
    # older/newer dats might use different keys:
    for key in ("bpmEvents", "_bpmEvents", "_BPMEvents", "_BPMChanges", "_bpmChanges", "BPMChanges"):
        if key in data and isinstance(data[key], list) and len(data[key]) > 0:
            # normalize to dicts with 'b' and 'm' if possible
            events = []
            for e in data[key]:
                # Common shapes: { "b": beat, "m": bpm } or { "_b": beat, "_m": bpm }
                if "b" in e and "m" in e:
                    events.append({"b": float(e["b"]), "m": float(e["m"])})
                elif "_b" in e and "_m" in e:
                    events.append({"b": float(e["_b"]), "m": float(e["_m"])})
            if events:
                return sorted(events, key=lambda x: x["b"])
    # fallback: single bpm in top-level maybe under "bpm" or "_beatsPerMinute"
    for key in ("bpm", "_beatsPerMinute", "beatsPerMinute"):
        if key in data:
            return [{"b": 0.0, "m": float(data[key])}]
    return None

def beats_to_seconds(beat, bpm_events):
    """
    Convert a beat position (float) into absolute seconds using bpm_events list:
    bpm_events = [ {"b": beat_at_event, "m": bpm_at_event}, ... ] sorted by b ascending.
    """
    if not bpm_events or len(bpm_events) == 0:
        raise ValueError("No bpm events provided")

    t = 0.0
    # Walk through segments
    for i in range(len(bpm_events)):
        cur = bpm_events[i]
        cur_b = cur["b"]
        cur_m = cur["m"]
        # If this is the last event, we may compute remaining part directly
        next_b = bpm_events[i+1]["b"] if i+1 < len(bpm_events) else None

        if beat < cur_b:
            # Beat is before the first event (rare) -> negative; treat as zero offset from start
            return 0.0

        if next_b is None or beat < next_b:
            # the target beat is within this BPM segment
            delta_beats = beat - cur_b
            t += (delta_beats * 60.0) / cur_m
            return t
        else:
            # add whole segment from cur_b to next_b, then continue
            delta_beats = next_b - cur_b
            t += (delta_beats * 60.0) / cur_m
            # continue loop
    # If we get here, beat is after last event; compute remainder with last bpm
    last = bpm_events[-1]
    delta_beats = beat - last["b"]
    t += (delta_beats * 60.0) / last["m"]
    return t

# -------------------- LOAD FILES --------------------
with open(input_file, "r", encoding="utf-8-sig") as f:
    data = json.load(f)

bpm_events = find_bpm_events(data)

# If not found inside the .dat, try Info.dat
if not bpm_events:
    info_path = os.path.join(os.path.dirname(input_file), "Info.dat")
    if os.path.exists(info_path):
        with open(info_path, "r", encoding="utf-8-sig") as f:
            info_data = json.load(f)
        if "_beatsPerMinute" in info_data:
            bpm_events = [{"b": 0.0, "m": float(info_data["_beatsPerMinute"])}]

# Fallback manual BPM (set this if Info.dat not present)
if not bpm_events:
    bpm_events = [{"b": 0.0, "m": 128.0}]  # <-- change 120 to your song's BPM
    print("⚠️ No BPM found in .dat or Info.dat, using fallback BPM = 120")

# ensure bpm_events sorted
bpm_events = sorted(bpm_events, key=lambda x: x["b"])


# -------------------- AUDIO ANALYSIS --------------------
y, sr = librosa.load(audio_file, sr=SR)
song_duration = librosa.get_duration(y=y, sr=sr)

# onset strength (energy envelope) for biasing
onset_env = librosa.onset.onset_strength(y=y, sr=sr, hop_length=HOP_LENGTH)
times = librosa.frames_to_time(np.arange(len(onset_env)), sr=sr, hop_length=HOP_LENGTH)
if onset_env.max() > onset_env.min():
    energy_curve = (onset_env - onset_env.min()) / (onset_env.max() - onset_env.min())
else:
    energy_curve = np.ones_like(onset_env) * 0.5

# -------------------- CONVERT NOTES (hit time -> spawn time) --------------------
raw_notes = data.get("_notes") or data.get("colorNotes") or data.get("notes") or []
all_notes = []
skipped_count = 0
for n in raw_notes:
    # supporting both rasputin old keys ("_time") and new ones ("b")
    beat_val = None
    if "_time" in n:
        beat_val = float(n["_time"])
    elif "b" in n:
        beat_val = float(n["b"])
    else:
        continue

    # convert beat -> hit_time (seconds), respecting BPM events (variable tempo)
    hit_time = beats_to_seconds(beat_val, bpm_events)

    # skip notes whose hit time is after audio duration (within margin)
    if hit_time > (song_duration - END_MARGIN):
        skipped_count += 1
        continue

    # spawn earlier so the note will reach player on hit
    spawn_time = max(0.0, hit_time - TRAVEL_TIME)

    # map line index -> 3-lane system
    # support older keys "_lineIndex" or "x"
    line_index = None
    if "_lineIndex" in n:
        line_index = int(n["_lineIndex"])
    elif "x" in n:
        line_index = int(n["x"])
    else:
        line_index = 1  # fallback center

    # clamp mapping: 0->-1, 1->0, 2/3->1
    if line_index == 0:
        lane = -1
    elif line_index == 1:
        lane = 0
    else:
        lane = 1

    # color mapping: old uses "_type" (0=red,1=blue), new uses "c" (0 red 1 blue)
    color = "red"
    if "_type" in n:
        color = "red" if int(n["_type"]) == 0 else "blue"
    elif "c" in n:
        color = "red" if int(n["c"]) == 0 else "blue"
    elif "a" in n and int(n["a"]) in (0,1):
        color = "red" if int(n["a"]) == 0 else "blue"

    # energy weight (based on hit_time)
    idx = np.searchsorted(times, hit_time)
    if idx >= len(energy_curve):
        idx = len(energy_curve) - 1
    energy = float(energy_curve[idx])

    all_notes.append({
        "hit_time": hit_time,
        "time": round(spawn_time, 3),
        "lane": lane,
        "color": color,
        "energy": energy
    })

print(f"Raw notes read: {len(raw_notes)} -> kept {len(all_notes)}, skipped {skipped_count} due to ending after audio.")

# -------------------- SMART SAMPLING (biased by energy) --------------------
total_raw = len(all_notes)
if total_raw == 0:
    raise ValueError("No usable notes found within audio duration. Check BPM/audio alignment.")

# if there are fewer notes than MIN_NOTES, we'll keep them all
if total_raw <= MIN_NOTES:
    final_notes = all_notes
else:
    target_count = random.randint(MIN_NOTES, MAX_NOTES)
    # compute weights
    weights = np.array([n["energy"] for n in all_notes], dtype=float)
    if weights.sum() <= 1e-8:
        probs = None  # fallback to uniform
    else:
        probs = weights / weights.sum()

    # choose indices without replacement using weights when possible
    if probs is None:
        chosen_idx = np.random.choice(len(all_notes), size=target_count, replace=False)
    else:
        chosen_idx = np.random.choice(len(all_notes), size=target_count, replace=False, p=probs)

    chosen_idx = sorted(chosen_idx)
    final_notes = [all_notes[i] for i in chosen_idx]

# Strip helper fields and sort by spawn time
for n in final_notes:
    n.pop("energy", None)
    n.pop("hit_time", None)
final_notes.sort(key=lambda x: x["time"])

# -------------------- SAVE --------------------
unity_json = {"notes": final_notes}
with open(output_file, "w", encoding="utf-8") as f:
    json.dump(unity_json, f, indent=4)

print(f"✅ Saved {len(final_notes)} notes to {output_file}")
print(f"Song duration: {song_duration:.2f}s, travel_time: {TRAVEL_TIME:.2f}s")

# Paths
input_file = r"C:\Users\Dell\Downloads\124 (Rasputin (Funk Overload) - jobas)\Hard.dat"
output_file = r"D:\Saksham\Unity\Projects\Gedit\VR-Beat-Saber-V2\Assets\Songs\Rasputin.json"
audio_file = r"C:\Users\Dell\Downloads\Rasputin.mp3"