#
# Install.ps1
#
#This script is a little ratchet but I'm not a powershell expert
param($installPath, $toolsPath, $package, $project)
#$project.Save()
#$project = $project.FullName



function get-relative-path ($source, $target)
{
	$tmp = Get-Location
	set-location $source
	$rtn = Resolve-Path -relative $target
	set-location $tmp
	Write-Host ("OUTPUT: " + $rtn)
	return $rtn
}

function verify-UsingTask([System.Xml.XmlDocument] $projectDocument, $projectDir, $toolsPath){
	Write-Host "Verifying task assembly reference...";
	$found = $false
	$relpath = get-relative-path $projectDir $toolsPath
	$dllPath = ($relpath + "\Mathtone.MIST.Builder.dll")
	Write-Host $dllPath
	ForEach ($node In $projectDocument.Project.UsingTask) {
		if($node.TaskName -eq "Mathtone.MIST.NotificationWeaverBuildTask"){
			$found = $true
			$node.AssemblyFile = $dllPath
			Write-Host ("Build task already referenced.  Updated to: " + $node.AssemblyFile)
		}
	}
	if(!$found){
		create-UsingTask $projectDocument $dllPath
	}
}

function verify-BuildTask([System.Xml.XmlDocument] $projectDocument){
	Write-Host "Verifying build task...";
	$found = $false
	ForEach ($target In $projectDocument.Project.Target) {
		if($target.Name -eq "AfterBuild"){
			ForEach($buildTask In $target){
				if($buildTask.NotificationWeaverBuildTask -ne $null){
					Write-Host "Found NotificationWeaverBuildTask"
					$found = $true
				}
			}
		}
	}
	
	if(!$found){
		Write-Host "Adding build task to project...";
		$targetElem = $projectDocument.CreateElement("Target","http://schemas.microsoft.com/developer/msbuild/2003")
		$targetElem.SetAttribute("Name","AfterBuild")
		$buildTaskElem = $projectDocument.CreateElement("NotificationWeaverBuildTask","http://schemas.microsoft.com/developer/msbuild/2003")
		$buildTaskElem.SetAttribute("TargetPath","`$(TargetPath)")
		$buildTaskElem.SetAttribute("DebugMode","True")
		$targetElem.AppendChild($buildTaskElem)
		$projectDocument.Project.AppendChild($targetElem)
	}
}

function create-UsingTask([System.Xml.XmlDocument] $projectDocument, $dllPath){
	Write-Host "Adding UsingTask to project...";
	$usingTaskElem = $projectDocument.CreateElement("UsingTask","http://schemas.microsoft.com/developer/msbuild/2003")
	$usingTaskElem.SetAttribute("TaskName","Mathtone.MIST.NotificationWeaverBuildTask")
	$usingTaskElem.SetAttribute("AssemblyFile",$dllPath)
	$projectDocument.Project.AppendChild($usingTaskElem)
}

$xdoc = new-object System.Xml.XmlDocument
$file = resolve-path $project
$xdoc.Load($file)
$projectDir = [System.IO.Path]::GetDirectoryName($project)
Write-Host $dllPath
verify-UsingTask $xdoc $projectDir $toolsPath
verify-BuildTask $xdoc
$xdoc.Save($project);
Write-Host "Complete"