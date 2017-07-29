require "fileutils"

def make_directory(directory)
    Dir.mkdir(directory) unless Dir.exist?(directory)
end

def remove_directory(directory)
    FileUtils.remove_dir(directory) if Dir.exist?(directory)
end

def delete_file(file)
    File.delete(file) if File.exist?(file)
end

def remove_unity_directory(directory)
    remove_directory(directory)
    delete_file("#{directory}.meta")
end
