#!/usr/bin/env ruby
require 'thor'
require 'mongo'
require 'mail'

include Mongo

class Drone < Thor
  @@drone_current_folder = "/deploy/drones/current/Out/Drone/"

  desc "start masterhost", "start the drone, defaults to xomixfuture"

  def start(master_host_name="")
    if master_host_name == ""
      master_host_name = "http://www.xomixfuture.com"
    end

    puts "Starting drone on #{master_host_name}"
    drone = fork do
      exec "mono #{@@drone_current_folder}/SpeedyMailer.Drones.exe -s#{master_host_name}"
    end

    Process.detach(drone)
  end

  desc "stop", "Stops the drone"

  def stop()
    `ps aux | grep mono | grep -v grep | awk '{print $2}' | xargs kill -9`
  end

  desc "logs", "View drone logs in bim"

  def logs()
    system("vim #{@@drone_current_folder}/logs/drone.txt")
  end

  desc "free", "Deletes old releases"

  def free()
    FileUtils.rm_f("/deploy/drones/releases")
  end

  desc "mongo", "log on to mongo"

  def mongo()
    system("mongo localhost:27027/drone")
  end

  desc "drop_database", "drops the drone database"

  def drop_database()
    MongoClient.new("localhost", 27027).drop_database("drone")
  end

  desc "test_email", "send a test email to david.mazvovsky@gmail.com"

  def test_email()
    options = {:address => "127.0.0.1",
               :port => 25}
    Mail.defaults do
      delivery_method :smtp, options
    end

    mail = Mail.new do
      from 'somedude@otherdomain.com'
      to 'admin@domain.com'
      subject 'This is a test email'
      body File.read('body.txt')
    end

    puts mail.to_s

    mail.deliver!
  end

end

Drone.start
