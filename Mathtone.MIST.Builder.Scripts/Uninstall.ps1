#
# Uninstall.ps1
#
#This script is a little ratchet but I'm not a powershell expert
param($installPath, $toolsPath, $package, $project)
$project.Save()
$projectName = $project.FullName

function get-relative-path ($source, $target)
{
	$tmp = Get-Location
	set-location $source
	$rtn = Resolve-Path -relative $target
	set-location $tmp
	Write-Host ("OUTPUT: " + $rtn)
	return $rtn
}

function remove-BuildTask([System.Xml.XmlDocument] $projectDocument){
	$found = $false
	ForEach ($target In $projectDocument.Project.Target) {
		if($target.Name = "AfterBuild"){
			ForEach ($buildTask In $target) {
				if($buildTask.Name = "NotificationWeaverBuildTask"){
					$found = $true
					Write-Host "Removing build task from project...";
					$target.RemoveChild($buildTask)
				}
			}
		}
	}
}

function remove-UsingTask([System.Xml.XmlDocument] $projectDocument, $projectDir, $toolsPath){
	$found = $false
	$relpath = get-relative-path $projectDir $toolsPath
	$dllPath = ($relpath + "\Mathtone.MIST.Builder.dll")
	Write-Host $dllPath
	ForEach ($node In $projectDocument.Project.UsingTask) {
		if($node.TaskName = "Mathtone.MIST.NotificationWeaverBuildTask"){
			Write-Host ("Removing task reference " + $node.Name)
			$projectDocument.Project.RemoveChild($node);
		}
	}
}

$xdoc = new-object System.Xml.XmlDocument
$file = resolve-path $projectName
$xdoc.Load($file)
$projectDir = [System.IO.Path]::GetDirectoryName($projectName)
Write-Host $dllPath
remove-UsingTask $xdoc $projectDir $toolsPath
remove-BuildTask $xdoc
Write-Host "1"
$project.Save();
Write-Host "2"
$xdoc.Save($projectName);
Write-Host "3"
Write-Host "Complete"