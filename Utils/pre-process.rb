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

lines = lines.map{|i| i.downcase}.uniq

#remove common email prefixes
email_prefixes = ["info", "sales", "admin", "mail@", "enquiries", "hello@", "copyright", "press", "company", "business", "www", "html"]
address_not_allowed = ["..", "-","_"," "]
domains_not_allowed = ["localhost"]

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

  if email_prefixes.any? { |prefix| address.include?(prefix) }
    next true
  end
  if address_not_allowed.any? { |prefix| address.include?(prefix) }
    next true
  end

  if domains_not_allowed.any? { |prefix| address.include?(prefix) }
    next true
  end
end

File.open(data_file + ".processed.txt", 'w') { |file| lines.each { |line| file.write(line) } }



