
data_file = ARGV.first
lines = Array.new

file = File.new(data_file, "r")
while (line = file.gets)
  lines << line
end
file.close

addresses = lines.map { |line| line.split('@')[0] }

more_then_once = addresses.select do |address|
  times = addresses.count { |x| x == address }
  if times > 1
    next true
  end
  next false
end

File.open(data_file + ".multiple.addresses.txt", 'w') { |file| more_then_once.each { |line| file.write(line + "\n") } }