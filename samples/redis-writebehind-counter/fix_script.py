import sys

p = 'F:/Development/DotNet/Usman Architecture/samples/redis-writebehind-counter/CounterService/CounterDrainJob.cs'
with open(p, 'r') as f:
    lines = f.readlines()

fixed = False
for i, line in enumerate(lines):
    if ':(batch.Granularity)}:' in line:
        lines[i] = line.replace(':(batch.Granularity)}:', ':{')
        fixed = True
        print(f"Fixed line {i+1}")
        break

if not fixed:
    print("Pattern not found!", file=sys.stderr)
    sys.exit(1)

with open(p, 'w') as f:
    f.writelines(lines)

print("Done")
