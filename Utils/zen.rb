data_file = ARGV.first

lines = Array.new

file = File.new(data_file, "r")
while (line = file.gets)
  lines << line
end
file.close

lines = lines.map { |i| i.downcase }
lines = lines.select { |line| line.length > 4 }

term_that_match = Array.new

counter = 0
lines.each do |line|

  if counter % 1000 == 0
    puts "we have processed: #{counter}"
  end

  matches_from_the_lines = lines.select { |x| x.include?(line) }
  if matches_from_the_lines.size > 1
    term_that_match = term_that_match + matches_from_the_lines
  end

  counter=counter+1

end

unique_matched_names = lines - term_that_match

File.open(data_file + ".unique.match.txt", 'w') { |file| unique_matched_names.each { |line| file.write(line) } }
File.open(data_file + ".matched.txt", 'w') { |file| term_that_match.each { |line| file.write(line) } }