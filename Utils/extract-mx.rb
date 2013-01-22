data_file = ARGV.first
call_type = ARGV[1]

mx_lines = Array.new
full_lines = Array.new

file = File.new(data_file, "r")
while (line = file.gets)
  matches = line.scan(/The domain: (.+?) has mx records:\s(.+?)$/)
  full_lines << line

  if matches.any?
    mxs = matches[0][1]
    mxRecords = mxs.split(', ')
    mx_lines.concat(mxRecords)
  end
end
file.close

mx_lines.sort!()

if call_type == "flatten"
  File.open(data_file + ".flatter.txt", 'w') { |file| mx_lines.each { |line| file.write(line + "\n") } }
end

if call_type == "spam"
  lines = full_lines.select { |line| line.include? 'spam' }
  File.open(data_file + ".spam.txt", 'w') { |file| lines.each { |line| file.write(line) } }
end



