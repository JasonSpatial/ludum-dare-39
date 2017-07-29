require "io/console"
require "optparse"
require "ostruct"
require_relative "BuildSystem/filesystemutils"

Settings = Struct.new(
    :projectDir,
    :tempDir,
    :logFile,
    :testsDir,
    :buildDir,
    :libraryDir,
    :tiled2UnityDir,
    :versionFile,
    :itchio_production_username,
    :itchio_production_game,
    :itchio_production_project)

BuildTarget = Struct.new(
    :name,
    :method,
    :directory,
    :itchio_channel)

build_targets = Hash[
    "webgl" => BuildTarget.new("WebGL", "Build.Build_WebGL", "Build/WebGL", "html5")
]

$settings = Settings.new()
$settings.projectDir = __dir__
$settings.tempDir = "#{$settings.projectDir}/Temp"
$settings.logFile = "#{$settings.projectDir}/editor.log"
$settings.testsDir = "#{$settings.projectDir}/Tests"
$settings.buildDir = "#{$settings.projectDir}/Build"
$settings.libraryDir = "#{$settings.projectDir}/Library"
$settings.tiled2UnityDir = "#{$settings.projectDir}/Assets/Tiled2Unity"
$settings.versionFile = "#{$settings.projectDir}/version.txt"

$settings.itchio_production_username = "michaelgrand"
$settings.itchio_production_game = "running-from-power"
$settings.itchio_production_project = "#{$settings.itchio_production_username}/#{$settings.itchio_production_game}"

$options = OpenStruct.new
$options.clean = false
$options.build_target = "webgl"
$options.deploy = false
$options.deployment_site = "none"
$options.nowait = true
OptionParser.new do |opts|
    opts.banner = "Usage: build.rb [options]"

    opts.separator ""
    opts.separator "Specific options:"

    opts.on("-c", "--clean", "Remove build artifacts/temporary files before building.") do |clean|
        $options.clean = clean
    end

    opts.on("-b", "--build=TARGET", "Sets the build TARGET (windows, osx, linux, webgl). Default: windows") do |target|
        target.downcase!
        if build_targets.has_key?(target)
            $options.build_target = target
        else
            puts("Invalid build target.")
            exit(1)
        end
    end

    opts.on("-d", "--deploy=SITE", "Deploy to SITE (none, production) after building. Default: none") do |site|
        site.downcase!
        if ["none", "production"].include?(site)
            $options.deploy = true unless site == "none"
            $options.deployment_site = site
        else
            puts("Invalid deployment site.")
            exit(1)
        end
    end

    opts.on("-w", "--nowait", "Don't pause before exiting.") do |v|
        $options.nowait = v
    end

    opts.separator ""
    opts.separator "Common options:"

    opts.on_tail("-h", "--help", "Show this message") do
        puts opts
        exit
    end
end.parse!

def clean_environment
    puts("\nCleaning up build artifacts...")

    remove_directory($settings.tempDir)
    remove_directory($settings.testsDir)
    remove_directory($settings.buildDir)
    remove_directory($settings.libraryDir)

    remove_unity_directory("#{$settings.tiled2UnityDir}/Materials")
    remove_unity_directory("#{$settings.tiled2UnityDir}/Meshes")
    remove_unity_directory("#{$settings.tiled2UnityDir}/Prefabs")
    remove_unity_directory("#{$settings.tiled2UnityDir}/Textures")

    delete_file($settings.logFile)

    puts("  Succeeded")
end

def setup_environment
    puts("\nSetting up build environment...")

    make_directory($settings.tempDir)
    make_directory($settings.testsDir)
    make_directory($settings.buildDir)

    puts("  Succeeded")
end

def shutdown
    if $options.nowait
        exit(0)
    else
        puts("\nPress Any Key to Continue...")
        char = STDIN.getch
        if char == "\u0003"
            exitCode = 1
        else
            exitCode = 0;
        end
        exit(exitCode)
    end
end

def run_editor_tests
    puts("\nRunning editor tests...")

    testResultsFilename = "#{$settings.testsDir}/editmode_test_results.xml"
    success = system("unity -batchmode -nographics -logFile \"#{$settings.logFile}\" -projectPath \"#{$settings.projectDir}\" -runTests -testResults \"#{testResultsFilename}\" -testPlatform editmode")
    if success
        puts("  Passed")
    else
        puts("  Failed")
        puts("  Details: #{testResultsFilename}")
        shutdown
    end
end

def run_playmode_tests
    puts("\nRunning play mode tests...")

    testResultsFilename = "#{$settings.testsDir}/playmode_test_results.xml"
    success = system("unity -batchmode -logFile \"#{$settings.logFile}\" -projectPath \"#{$settings.projectDir}\" -runTests -testResults \"#{testResultsFilename}\" -testPlatform playmode")
    if success
        puts("  Passed")
    else
        puts("  Failed")
        puts("  Details: #{testResultsFilename}")
        shutdown
    end
end

def build(target, build_targets)
    if !build_targets.has_key?(target)
        puts("Invalid build target.")
        shutdown
    end

    puts("\nCreating #{build_targets[target].name} build...")
    success = system("unity -quit -batchmode -nographics -logFile \"#{$settings.logFile}\" -projectPath \"#{$settings.projectDir}\" -executeMethod #{build_targets[target].method}")
    if success
        puts("  Succeeded")
    else
        puts("  Failed")
        puts("  Details: #{$settings.logFile}")
        shutdown
    end
end

def deploy_to_production_site(target, build_targets)
    puts("\nDeploying to production site...")

    success = system("butler push \"#{build_targets[target].directory}\" #{$settings.itchio_production_project}:#{build_targets[target].itchio_channel} --userversion-file=\"#{$settings.versionFile}\"")
    if success
        puts("  Succeeded")
    else
        puts("  Failed")
        shutdown
    end
end

clean_environment if $options.clean
setup_environment
run_editor_tests
# Disabled pending a Unity patch. Playmode tests litter the Assets directory with "InitTestScene" scenes.
#run_playmode_tests
build($options.build_target, build_targets)
deploy_to_production_site($options.build_target, build_targets) if $options.deploy && $options.deployment_site == "production"
shutdown