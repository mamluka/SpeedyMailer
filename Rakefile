require 'albacore'
require 'nokogiri'
require 'open-uri'
require 'fileutils'
require 'socket'

namespace :windows do

  DRONE_SOLUTION_FILE = "SpeedyMailer.Drones\\SpeedyMailer.Drones.csproj"
  DRONE_OUTPUT_FOLDER = "..\\Out\\Drone"

  SERVICE_SOLUTION_FILE = "SpeedyMailer.Master.Service\\SpeedyMailer.Master.Service.csproj"
  SERVICE_OUTPUT_FOLDER = "..\\Out\\Service"
  
  API_SOLUTION_FILE = "SpeedyMailer.Master.Web.Api\\SpeedyMailer.Master.Web.Api.csproj"
  API_OUTPUT_FOLDER = "C:\\SpeedyMailer\\Api"

  APP_FOLDER = "SpeedyMailer.Master.Web.App\\SpeedyMailer.Master.Web.App.csproj"
  APP_OUTPUT_FOLDER = "C:\\SpeedyMailer\\App"

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
  
  desc "Publish website"
  msbuild :publish, [:solution,:output_folder] do |msb,args|
    msb.properties = { :configuration=>:Release }
	msb.targets [:Rebuild,:ResolveReferences,:_CopyWebApplication]
	msb.properties = {
			:webprojectoutputdir=> args[:output_folder],
			:outdir => args[:output_folder] + "\\bin"
		}
	msb.solution = args[:solution]
  end


  desc "Build drone from project file"
  task :build_drone do
    puts "Start building drone..."
    Rake::Task["windows:build"].invoke(DRONE_SOLUTION_FILE,DRONE_OUTPUT_FOLDER)
    end

  desc "Build service from project file"
  task :build_service do
    puts "Start building service..."
    Rake::Task["windows:build"].invoke(SERVICE_SOLUTION_FILE,SERVICE_OUTPUT_FOLDER)
  end
  
  desc "Build API"
  task :build_api do
    puts "Start building api..."
    Rake::Task["windows:publish"].invoke(API_SOLUTION_FILE,API_OUTPUT_FOLDER)
  end
  
  desc "Build App"
  task :build_app do
    puts "Start building app..."
		FileUtils.cp_r '', 'target'
	end
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

    exec :run_raven do |cmd|
	  puts "All data will be deleted..."
	  FileUtils.rm_rf 'RavenDb\\Server\\Data'
	
      cmd.command="cmd.exe"
      cmd.parameters=["/c","start","RavenDb\\Server\\Raven.Server.exe"]
    end

    task :update_raven_url do |t|
      puts "Opening app.config file for writing..."

      f = File.open("SpeedyMailer.Master.Service\\app.config")
      xml = Nokogiri::XML(f)
      f.close()

      connString = xml.xpath("//configuration/connectionStrings/add").first()
      connString["connectionString"] =  "Url = http://localhost:4253"

      puts "Updating app.config with the new raven url"
      File.open("SpeedyMailer.Master.Service\\app.config",'w') {|f| xml.write_xml_to f}
    end

    desc "Run service with database"

    exec :run_service,[:host] => [:update_raven_url,"windows:build_service",:run_raven] do |cmd,args|
      cmd.command="cmd.exe"
      cmd.parameters=["/c","start","Out\\Service\\SpeedyMailer.Master.Service.exe","-b",args[:host]]
      cmd.log_level = :verbose
    end
	
	desc "Run service bare"

    exec :run_service_bare => ["windows:build_service"] do |cmd|
      cmd.command="cmd.exe"
      cmd.parameters=["/c","start","Out\\Service\\SpeedyMailer.Master.Service.exe","-b",local_host_url]
      cmd.log_level = :verbose
    end

  desc "Run default service"

  task :run_default_service do
    puts local_ip
	
    Rake::Task["winrun:run_service"].invoke(local_host_url)
  end
  
  def local_host_url
      "http://"+local_ip
  end
  
  def local_ip
    orig, Socket.do_not_reverse_lookup = Socket.do_not_reverse_lookup, true  # turn off reverse DNS resolution temporarily

    UDPSocket.open do |s|
      s.connect '64.233.187.99', 1
      s.addr.last
    end
  ensure
    Socket.do_not_reverse_lookup = orig
  end
end



