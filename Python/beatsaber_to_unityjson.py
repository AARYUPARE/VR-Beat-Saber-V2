import json

# Load Beat Saber file
with open("Animals_beatsaber_raw.json", "r") as f:
    data = json.load(f)

# Get BPM
bpm = data["bpmEvents"][0]["m"]
beat_duration = 60.0 / bpm

notes = []
for n in data["colorNotes"]:
    time_sec = n["b"] * beat_duration

    # Map Beat Saber x grid (0-3) to lanes (-1, 0, 1)
    # Example: collapse 4 columns into 3 lanes
    if n["x"] in [0, 1]:
        lane = -1
    elif n["x"] == 2:
        lane = 0
    else:  # n["x"] == 3
        lane = 1

    color = "red" if n["c"] == 0 else "blue"

    notes.append({
        "time": round(time_sec, 2),
        "lane": lane,
        "color": color
    })

# Wrap
output = {"notes": notes}

# Save
with open("converted_beatmap.json", "w") as f:
    json.dump(output, f, indent=4)

print(f"✅ Converted {len(notes)} notes into Unity format")
