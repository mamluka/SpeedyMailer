require 'point'

API_KEY = "93bc6509-f7d6-1838-511a-68dcfef5ed56"

my_domain = "cookiexfactory.info"
my_ip = "147.254.25.19"

file = File.open("dkim-dns.txt", "rb")
open_dkim_file_content = file.read
file.close

open_dkim_dns_entry = open_dkim_file_content.scan(/"(.+?)"/)[0]

file = File.open("domain-keys-dns.txt", "rb")
domain_keys_file_content = file.read
file.close

domain_keys_dns_entry = domain_keys_file_content.scan(/-----BEGIN PUBLIC KEY-----(.+?)-----END PUBLIC KEY-----/m)[0][0].gsub("\r\n","")

Point.username = "jdeering@centracorporation.com"
Point.apitoken = API_KEY

zones = Point::Zone.find(:all)

my_zone_id = 0
zones.each do |zone|
  my_zone_id = zone.name == my_domain ? zone.id : 0
end

puts my_zone_id

zone = Point::Zone.find(my_zone_id)

zone.records.each do |record|
   record.destroy
end

#add A record
new_record = zone.build_record
new_record.record_type = "A"
new_record.name = ""
new_record.data = my_ip
puts new_record.save

#add mail record
new_record = zone.build_record
new_record.record_type = "A"
new_record.name = "mail"
new_record.data = my_ip
puts new_record.save

#add mx record
new_record = zone.build_record
new_record.record_type = "MX"
new_record.name = ""
new_record.data = "mail." + my_domain
new_record.aux = "5"
puts new_record.save

#add ptr record
new_record = zone.build_record
new_record.record_type = "PTR"
new_record.name = my_ip.split(".").reverse!.join(".") + ".in-addr.arpa."
new_record.data = "mail."+ my_domain
puts my_ip.split(".").reverse!.join(".") + ".in-addr.arpa"
puts new_record.save
puts new_record.errors