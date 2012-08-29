require 'albacore'

namespace :windows do

  SOLUTION_FILE = "SpeedyMailer.Drones\\SpeedyMailer.Drones.csproj"
  OUTPUT_FOLDER = "..\\Out\\Drone"

  desc "clean the solution"
  msbuild :clean do |msb|
    msb.targets :Clean
    msb.solution  = SOLUTION_FILE
  end

  desc "Build the solution"
  msbuild :build => :clean do |msb|
    msb.properties :configurations => :Release,:OutputPath => OUTPUT_FOLDER
    msb.targets :Rebuild
    msb.solution  = SOLUTION_FILE
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

namespace :run do

    BASE_FOLDER =  File.dirname(__FILE__)

    desc "Run ravendb server on the pre-configured url and port"

    exec :raven do |cmd|
      cmd.command="start"
      # cmd.parameters =  ["cmd" ,"/c","RavenDB\\Server\\Raven.Server.exe"]
      cmd.parameters = "cmd"
    end
end



