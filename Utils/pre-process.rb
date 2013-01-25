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

#remove common email prefixes
email_prefixes = ["events", "jobs", "marketing", "staff", "office", "careers", "feedback", "legal", "news", "editor", "noreply", "privacy", "public", "recruit", "registration", "repair", "report", "security", "showcase", "site", "spoof", "test", "tips", "repor", "training", "training", "unsubscribe", "volunteer", "abc", "xyz", "your", "blog", "billing", "enquiry", "question", "help", "affiliate", "advertis", "update", "service", "service", "webmaster", "contact", "customer", "info", "sales", "admin", "mail@", "enquiries", "hello@", "copyright", "press", "company", "business", "www", "html", "support", "foo"]
address_not_allowed = ["..", " ", "--"]
domains_not_allowed = ["localhost", "example.com", "email.com", "test"]

lines.delete_if do |line|
  line_split = line.split('@')
  address = line_split[0]

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

removed = all_lines-lines
removed = removed.uniq

File.open(data_file + ".processed.txt", 'w') { |file| lines.each { |line| file.write(line) } }
File.open(data_file + ".processed.removed.txt", 'w') { |file| removed.each { |line| file.write(line) } }



