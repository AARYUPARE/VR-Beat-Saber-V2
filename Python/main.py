import librosa
import json
import random
import numpy as np

# -------- Configuration ---------
filename = "Animals - Maroon 5 (Audio).mp3"
sr = 44100
lane_options = [-1, 0, 1]
color_options = ["red", "blue"]
note_speed = 10.0       # units/sec
spawn_z = 10
player_z = -10
target_notes_range = (80, 120)
subdivision_prob = 0.2   # fraction of notes that get a subdivision
min_lane_gap = 0.1       # seconds

# -------- Load song ---------
y, sr = librosa.load(filename, sr=sr)
song_duration = librosa.get_duration(y=y, sr=sr)

# -------- Beat and onset detection ---------
tempo, beat_frames = librosa.beat.beat_track(y=y, sr=sr)
beat_times = librosa.frames_to_time(beat_frames, sr=sr)

onset_frames = librosa.onset.onset_detect(y=y, sr=sr)
onset_times = librosa.frames_to_time(onset_frames, sr=sr)

# Combine and sort candidate times
candidate_times = np.unique(np.concatenate([beat_times, onset_times]))
candidate_times = np.sort(candidate_times)

# -------- Calculate travel time ---------
distance = spawn_z - player_z
travel_time = distance / note_speed

# -------- Determine number of notes ---------
target_notes = random.randint(*target_notes_range)
if target_notes > len(candidate_times):
    # if fewer candidate times than target, allow subdivisions
    candidate_times = np.repeat(candidate_times, 2)  # allow duplicates

# -------- Sample evenly spaced notes ---------
step = max(1, len(candidate_times) // target_notes)
notes = []
last_lane_times = {l: -np.inf for l in lane_options}

for i in range(0, len(candidate_times), step):
    if len(notes) >= target_notes:
        break
    t = candidate_times[i]
    spawn_time = max(0, t - travel_time)

    # choose lane not used too recently
    possible_lanes = [l for l in lane_options if spawn_time - last_lane_times[l] >= min_lane_gap]
    if not possible_lanes:
        continue
    lane = random.choice(possible_lanes)
    color = random.choice(color_options)
    notes.append({"time": round(spawn_time, 2), "lane": lane, "color": color})
    last_lane_times[lane] = spawn_time

    # Optional subdivision
    if i + 1 < len(candidate_times) and random.random() < subdivision_prob and len(notes) < target_notes:
        next_t = candidate_times[i + 1]
        sub_time = t + (next_t - t) / 2
        sub_spawn = max(0, sub_time - travel_time)
        sub_lanes = [l for l in lane_options if sub_spawn - last_lane_times[l] >= min_lane_gap]
        if sub_lanes:
            sub_lane = random.choice(sub_lanes)
            sub_color = random.choice(color_options)
            notes.append({"time": round(sub_spawn, 2), "lane": sub_lane, "color": sub_color})
            last_lane_times[sub_lane] = sub_spawn

# Sort notes by time
notes.sort(key=lambda x: x["time"])

# -------- Save JSON ---------
beatmap = {"notes": notes}
output_file = "generated_beatmap_fullsong.json"
with open(output_file, "w") as f:
    json.dump(beatmap, f, indent=4)

print(f"✅ Beatmap saved to {output_file}")
print(f"Estimated BPM: {tempo}")
print(f"Total notes: {len(notes)}")
print(f"Song duration: {song_duration:.2f} seconds")
