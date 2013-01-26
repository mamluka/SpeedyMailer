class String
  def numeric?
    Float(self) != nil rescue false
  end
end

data_file = ARGV.first

lines = Array.new

file = File.new(data_file, "r")
while (line = file.gets)
  lines << line
end
file.close

all_lines = lines.clone
lines = lines.map { |i| i.downcase }.uniq

email_prefixes = Array.new

file = File.new("bad.email.prefixes.txt", "r")
while (line = file.gets)
  email_prefixes << line[0..-2]
end
file.close

puts email_prefixes.length
#remove common email prefixes
address_not_allowed = ["..", " ", "--",'-']
domains_not_allowed = ["localhost", "example", "email.com", "test","emailaddress.com","domain.com"]

lines.delete_if do |line|
  line_split = line.split('@')
  address = line_split[0]
  domain = line_split[1]

  if address.length == 1
    next true
  end

  if address.numeric?
    next true
  end

  if address.scan(/\d/).size > 2
    next true
  end

  if email_prefixes.any? { |x| address.include?(x) }
    next true
  end
  if address_not_allowed.any? { |x| address.include?(x) }
    next true
  end

  if domains_not_allowed.any? { |x| domain.include?(x) }
    next true
  end
end

removed = all_lines-lines
removed = removed.uniq

File.open(data_file + ".processed.txt", 'w') { |file| lines.each { |line| file.write(line) } }
File.open(data_file + ".processed.removed.txt", 'w') { |file| removed.each { |line| file.write(line) } }



