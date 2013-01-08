require 'albacore'
require 'nokogiri'
require 'open-uri'
require 'socket'
require 'json'

require 'sys/proctable'
include Sys

namespace :windows do

  MASTER_PREDEPLOY_FOLDER = "C:/SpeedyMailer/Release"
  WIN32_MASTER_DEPLOY_FOLDER = "C:\\SpeedyMailer"

  MASTER_DEPLOY_FOLDER = "C:/SpeedyMailer"

  BACKUP_FOLDER = "C:/Backup"

  DRONE_SOLUTION_FILE = "SpeedyMailer.Drones/SpeedyMailer.Drones.csproj"
  DRONE_OUTPUT_FOLDER = "../Out/Drone"

  SERVICE_SOLUTION_FILE = "SpeedyMailer.Master.Service/SpeedyMailer.Master.Service.csproj"
  SERVICE_OUTPUT_FOLDER = MASTER_PREDEPLOY_FOLDER + "/Service"

  RAY_SOLUTION_FILE = "SpeedyMailer.Master.Ray/SpeedyMailer.Master.Ray.csproj"
  RAY_OUTPUT_FOLDER = MASTER_PREDEPLOY_FOLDER + "/Ray"

  DEPLOY_SOLUTION_FILE = "SpeedyMailer.Master.Deploy/SpeedyMailer.Master.Deploy.csproj"
  DEPLOY_OUTPUT_FOLDER = MASTER_PREDEPLOY_FOLDER + "/Deploy"

  DEPLOY_EXE = MASTER_PREDEPLOY_FOLDER + "/Deploy/SpeedyMailer.Master.Deploy.exe"

  API_SOLUTION_FILE = "SpeedyMailer.Master.Web.Api/SpeedyMailer.Master.Web.Api.csproj"
  API_OUTPUT_FOLDER = "Api"

  APP_FOLDER = "SpeedyMailer.Master.Web.App/static/app"
  APP_OUTPUT_FOLDER = "App"

  MASTER_DOMAIN = "xomixfuture.com"

  desc "Clean solution"
  msbuild :clean, [:solution] do |msb, args|
    msb.targets :Clean
    msb.solution = args[:solution]
  end

  desc "Build solution"
  msbuild :build, [:solution, :output_folder] do |msb, args|
    msb.properties :configurations => :Release, :OutputPath => args[:output_folder]
    msb.targets :Rebuild
    msb.solution = args[:solution]
  end

  desc "Build solution"
  msbuild :build2, [:solution, :output_folder] do |msb, args|
    msb.properties :configurations => :Release, :OutputPath => args[:output_folder]
    msb.targets :Rebuild
    msb.solution = args[:solution]
  end

  desc "Publish website"
  msbuild :publish, [:solution, :output_folder] do |msb, args|
    msb.properties = {:configuration => :Release}
    msb.targets [:Rebuild, :ResolveReferences, :_CopyWebApplication]
    msb.properties = {
        :webprojectoutputdir => File.join(MASTER_PREDEPLOY_FOLDER, args[:output_folder]),
        :outdir => File.join(MASTER_PREDEPLOY_FOLDER, args[:output_folder], "bin/")
    }
    msb.solution = args[:solution]
  end

  desc "Build drone from project file"
  task :build_drone do
    puts "Start building drone..."
    Rake::Task["windows:build"].invoke(DRONE_SOLUTION_FILE, DRONE_OUTPUT_FOLDER)
    Rake::Task["windows:build"].reenable
  end

  desc "Build service from project file"
  task :build_service do
    puts "Start building service..."
    Rake::Task["windows:build"].invoke(SERVICE_SOLUTION_FILE, SERVICE_OUTPUT_FOLDER)
    Rake::Task["windows:build"].reenable
    end

  desc "Build ray tool"
  task :build_ray do
    puts "Start building ray..."
    Rake::Task["windows:build"].invoke(RAY_SOLUTION_FILE, RAY_OUTPUT_FOLDER)
    Rake::Task["windows:build"].reenable
  end

  desc "Build API"
  task :build_api do
    puts "Start building api..."
    Rake::Task["windows:publish"].invoke(API_SOLUTION_FILE, API_OUTPUT_FOLDER)
    Rake::Task["windows:publish"].reenable
  end

  desc "Build App"
  task :build_app do
    puts "Start building app..."
    app_folder = File.join(MASTER_PREDEPLOY_FOLDER, APP_OUTPUT_FOLDER)
    FileUtils.mkdir_p(app_folder)
    FileUtils.cp_r APP_FOLDER + "\\.", app_folder
  end

  desc "Build deploy program"
  task :build_deploy do
    puts "Build deploy executable"
    Rake::Task["windows:build"].invoke(DEPLOY_SOLUTION_FILE, DEPLOY_OUTPUT_FOLDER)
    Rake::Task["windows:build"].reenable
  end

  #Deploy commands

  desc "Deploy service"
  task :deploy_service => [:build_service, :build_deploy, :run_raven, :exec_deploy_service] do
  end

  desc "Deploy api"
  task :deploy_api => [:build_api, :configure_api, :build_deploy, :exec_deploy_api] do
  end

  desc "Deploy app"
  task :deploy_app => [:build_app, :build_deploy, :configure_app, :exec_deploy_app] do
  end

  desc "Deploy master applications"
  task :deploy => [:deploy_service, :deploy_api, :deploy_app] do
  end
  
  desc "Deploy web only"
  task :deploy_web => [:deploy_api, :deploy_app] do
  end


  desc "Execute deployment of service"
  exec :exec_deploy_service do |cmd|
    puts "Launching deployment command for service"

    cmd.command=DEPLOY_EXE
    cmd.parameters=["--deploy-service", "--base-url", MASTER_DOMAIN, "--base-directory", WIN32_MASTER_DEPLOY_FOLDER]
  end

  desc "Deploy raven"
  task :upgrade_raven => [:shutdown_raven, :backup_raven_db, :copy_raven, :restore_raven_db, :run_raven] do
  end

  desc "Copy RavenDB"
  task :copy_raven do
    FileUtils.rm_rf MASTER_DEPLOY_FOLDER + "/Server"
    FileUtils.cp_r "RavenDB/Server", MASTER_DEPLOY_FOLDER + "/Server"
  end

  desc "Backup ravendb data"
  task :backup_raven_db do
    next if !File.directory?(MASTER_DEPLOY_FOLDER + "/Server")

    FileUtils.mkdir_p(BACKUP_FOLDER + "/Database")
    latest = BACKUP_FOLDER + "/Database/Latest"

    if File.directory?(latest)
      FileUtils.mv latest, BACKUP_FOLDER + "/Database/" + Time.now.strftime('%a-%b-%d-%H-%M-%S')
    end

    FileUtils.cp_r MASTER_DEPLOY_FOLDER + "/Server/Data", latest
  end

  desc "Restore latest data"
  task :restore_raven_db do
    latest = BACKUP_FOLDER + "/Database/Latest"
    if File.directory?(latest)
      FileUtils.cp_r latest, MASTER_DEPLOY_FOLDER + "/Server/Data"
    end
  end

  desc "Run raven"
  task :run_raven do
    ravenRunning = ProcTable.ps.any? { |p| p.name == "Raven.Server.exe" }

    if !ravenRunning
      Rake::Task["windows:exec_run_raven"].invoke
    end
  end

  desc "Shutdown raven"
  task :shutdown_raven do
    ProcTable.ps.each do |p|
      if p.name == "Raven.Server.exe"
         Process.kill('KILL',p.pid)
      end
    end
  end

  desc "Run raven32"
  exec :exec_run_raven  do |cmd|
    cmd.command="cmd.exe"
    cmd.parameters=["/c", "start", WIN32_MASTER_DEPLOY_FOLDER + "\\Server\\Raven.Server.exe"]
  end

  desc "Execute deployment of api"
  exec :exec_deploy_api do |cmd|
    puts "Launching deployment command for api"

    cmd.command=DEPLOY_EXE
    cmd.parameters=["--deploy-api", "--base-url", MASTER_DOMAIN, "--base-directory", WIN32_MASTER_DEPLOY_FOLDER]
  end

  desc "configure the app js file"
  task :configure_app do
    configFile = File.join(MASTER_PREDEPLOY_FOLDER, APP_OUTPUT_FOLDER, "js", "config.js")

    puts configFile
    FileUtils.rm_rf configFile;

    File.open(configFile, 'w') {
        |file| file.write("config = {}; config.apiUrl = 'http://api.#{MASTER_DOMAIN}';")
    }
  end

  desc "configure the app js file"
  task :configure_api do
    configFolder = File.join(MASTER_PREDEPLOY_FOLDER, API_OUTPUT_FOLDER, "settings")

    configFile = File.join(configFolder, "ApiCalls.settings")

    FileUtils.mkdir configFolder


    tempHash = {
        "ApiBaseUri" => "http://www.#{MASTER_DOMAIN}",
    }

    File.open(configFile, "w") do |f|
      f.write(tempHash.to_json)
    end

  end

  desc "Execute deployment of app"
  exec :exec_deploy_app do |cmd|
    puts "Launching deployment command for app"

    cmd.command=DEPLOY_EXE
    cmd.parameters=["--deploy-app", "--base-url", MASTER_DOMAIN, "--base-directory", WIN32_MASTER_DEPLOY_FOLDER]
  end

end

namespace :mono do

  MONO_SOLUTION_FILE = "SpeedyMailer.Mono.sln"
  MONO_OUTPUT_FOLDER = "../Out/Drone"

  desc "clean the solution"
  xbuild :clean do |msb|
    msb.targets :Clean
    msb.solution = MONO_SOLUTION_FILE
  end

  desc "Build the solution"
  xbuild :build => :clean do |msb|
    msb.properties :configurations => :Release, :OutputPath => MONO_OUTPUT_FOLDER
    msb.targets :Rebuild
    msb.solution = MONO_SOLUTION_FILE
  end
end

namespace :winrun do

  BASE_FOLDER = File.dirname(__FILE__)

  desc "Run ravendb server on the pre-configured url and port"

  exec :run_raven do |cmd|
    puts "All data will be deleted..."
    FileUtils.rm_rf 'RavenDb\\Server\\Data'

    cmd.command="cmd.exe"
    cmd.parameters=["/c", "start", "RavenDb\\Server\\Raven.Server.exe"]
  end

  task :update_raven_url do |t|
    puts "Opening app.config file for writing..."

    f = File.open("SpeedyMailer.Master.Service\\app.config")
    xml = Nokogiri::XML(f)
    f.close()

    connString = xml.xpath("//configuration/connectionStrings/add").first()
    connString["connectionString"] = "Url = http://localhost:4253"

    puts "Updating app.config with the new raven url"
    File.open("SpeedyMailer.Master.Service\\app.config", 'w') { |f| xml.write_xml_to f }
  end

  desc "Run service with database"

  exec :run_service, [:host] => [:update_raven_url, "windows:build_service", :run_raven] do |cmd, args|
    cmd.command="cmd.exe"
    cmd.parameters=["/c", "start", "Out\\Service\\SpeedyMailer.Master.Service.exe", "-b", args[:host]]
    cmd.log_level = :verbose

  end

  desc "Run service bare"

  exec :run_service_bare => ["windows:build_service"] do |cmd|
    cmd.command="cmd.exe"
    cmd.parameters=["/c", "start", "Out\\Service\\SpeedyMailer.Master.Service.exe", "-b", local_host_url]
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
    orig, Socket.do_not_reverse_lookup = Socket.do_not_reverse_lookup, true # turn off reverse DNS resolution temporarily

    UDPSocket.open do |s|
      s.connect '64.233.187.99', 1
      s.addr.last
    end
  ensure
    Socket.do_not_reverse_lookup = orig
  end
end



