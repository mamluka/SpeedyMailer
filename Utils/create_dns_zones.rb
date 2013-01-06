require 'point'
require 'net/http'
require 'uri'

API_KEY = "93bc6509-f7d6-1838-511a-68dcfef5ed56"

my_ip = Net::HTTP.get(URI.parse("http://ipecho.net/plain"))
my_domain = `/usr/bin/dig +noall +answer -x #{my_ip} | awk '{$5=substr($5,1,length($5)-1); print $5}' | tr  -d '\n'`

abort "There is not domain associated this ip" if my_domain.empty?

puts my_ip

file = File.open("/deploy/domain-keys/dkim-dns.txt", "rb")
open_dkim_file_content = file.read
file.close

open_dkim_dns_entry = open_dkim_file_content.scan(/"(.+?)"/)[0][0]

file = File.open("/deploy/domain-keys/domain-keys-dns.txt", "rb")
domain_keys_file_content = file.read
file.close

domain_keys_dns_entry = domain_keys_file_content.scan(/-----BEGIN PUBLIC KEY-----(.+?)-----END PUBLIC KEY-----/m)[0][0].gsub(/\n/,"")

Point.username = "jdeering@centracorporation.com"
Point.apitoken = API_KEY

zones = Point::Zone.find(:all)

my_zone = zones.find { |zone| zone.name == my_domain }
if my_zone.nil?
  zone = Point::Zone.new
  zone.name = my_domain
  zone.save
else
  zone = Point::Zone.find(my_zone.id)
end

#clean the records
zone.records.each do |record|
  record.destroy
end

#add A record
new_record = zone.build_record
new_record.record_type = "A"
new_record.name = ""
new_record.data = my_ip
puts new_record.save

#add A mail record
new_record = zone.build_record
new_record.record_type = "A"
new_record.name = "mail"
new_record.data = my_ip
puts new_record.save

#add www mail record
new_record = zone.build_record
new_record.record_type = "A"
new_record.name = "www"
new_record.data = my_ip
puts new_record.save

#add mx record
new_record = zone.build_record
new_record.record_type = "MX"
new_record.name = ""
new_record.data = "mail." + my_domain
new_record.aux = "5"
puts new_record.save



#add domain keys

new_record = zone.build_record
new_record.record_type = "TXT"
new_record.name = "maildk._domainkey"
new_record.data = "\"k=rsa; t=y; p=" + domain_keys_dns_entry + "\""
puts new_record.save

puts new_record.errors

#add dkim keys
new_record = zone.build_record
new_record.record_type = "TXT"
new_record.name = "mail._domainkey"
new_record.data = "\"" +  open_dkim_dns_entry + "\""
puts new_record.save

#ptr stuff maybe useful in the future

##check if the zone for this ip exists
#my_ip_reverse = my_ip.split(".").reverse!
#
#ptr_zone_ip = my_ip_reverse[1, 3].join(".")
#ptr_zone_name = ptr_zone_ip + ".in-addr.arpa"
#
#zone_exists = zones.any? { |zone| zone.name == ptr_zone_name }
#
##add zone if dosen't exist
#if zone_exists then
#  ptr_zone = zones.find{|zone| zone.name == ptr_zone_name}
#else
#  ptr_zone = Point::Zone.new
#  ptr_zone.name = ptr_zone_name
#  ptr_zone.save
#end
#
##add ptr record
#new_record = ptr_zone.build_record
#new_record.record_type = "PTR"
#new_record.name = my_ip_reverse.join(".") + ".in-addr.arpa."
#new_record.data = "mail."+ my_domain
#puts new_record.save
