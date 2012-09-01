require 'albacore'
require 'nokogiri'
require 'open-uri'

namespace :windows do

  DRONE_SOLUTION_FILE = "SpeedyMailer.Drones\\SpeedyMailer.Drones.csproj"
  DRONE_OUTPUT_FOLDER = "..\\Out\\Drone"

  desc "Clean solution"
  msbuild :clean, [:solution] do |msb,args|
    msb.targets :Clean
    msb.solution  = args[:solution]
  end

  desc "Build solution"
  msbuild :build, [:solution,:output_folder] do |msb,args|
    msb.properties :configurations => :Release,:OutputPath => args[:output_folder]
    msb.targets :Rebuild
    msb.solution  = args[:solution]
  end

  desc "Build drone from project file"
  task :build_drone do
    puts "Start building..."
    Rake::Task[windows:build].invoke(DRONE_SOLUTION_FILE,DRONE_OUTPUT_FOLDER)
  end
end

namespace :mono do

  MONO_SOLUTION_FILE = "SpeedyMailer.Mono.sln"
  MONO_OUTPUT_FOLDER = "../Out/Drone"

  desc "clean the solution"
  xbuild :clean do |msb|
    msb.targets :Clean
    msb.solution  = MONO_SOLUTION_FILE
  end

  desc "Build the solution"
  xbuild :build => :clean do |msb|
    msb.properties :configurations => :Release,:OutputPath => MONO_OUTPUT_FOLDER
    msb.targets :Rebuild
    msb.solution  = MONO_SOLUTION_FILE
  end
end

namespace :winrun do

    BASE_FOLDER =  File.dirname(__FILE__)

    desc "Run ravendb server on the pre-configured url and port"

    exec :raven do |cmd|
      cmd.command="cmd.exe"
      cmd.parameters=["/c","start","RavenDb\\Server\\Raven.Server.exe"]
    end

    task :update_raven_url,[:url] do |t,args|
      puts "Opening app.config file for writing..."

      f = File.open("SpeedyMailer.Master.Service\\app.config")
      xml = Nokogiri::XML(f)
      f.close()

      connString = xml.xpath("//configuration/connectionStrings/add").first()
      connString["connectionString"] =  "Url = #{args[:url]}"

      puts "Updating app.config with the new raven url"
      File.open("SpeedyMailer.Master.Service\\app.config",'w') {|f| xml.write_xml_to f}
    end






end



