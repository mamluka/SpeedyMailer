RAY_PATH = "C:\\SpeedyMailer\\Release\\Ray\\SpeedyMailer.Master.Ray.exe"

puts "Load data file"

data_file = ARGV.first

puts "Extract domains"
domain_file = data_file + ".domains.txt"
`#{RAY_PATH} -p #{data_file} -o #{domain_file} -x`

puts "Run DNS clean"
bad_domains = data_file + ".bad.domain.txt"
`#{RAY_PATH} -d #{domain_file} -o #{bad_domains}`

puts "Output clean list"
output_file = data_file + ".clean.txt"
`#{RAY_PATH} -p #{data_file} -o #{output_file} -b #{bad_domains}`

puts "Run name matches"
names_file = "names.to.match.txt"
names_matched = data_file + ".names.matched.txt"
`#{RAY_PATH} -p #{output_file} -o #{names_matched} -n #{names_file}`
