 param (
    [switch]$doAll  = $false
 )

$OutputDir = "UploadPackage";
$ConsoleRunner = "RandToeConsoleRunner";
$MonoBin = "'C:\Program Files (x86)\Mono\bin";

$RandToeEngine = "RandToeEngine";
$RandToeEngineBot = "Bots";
$RandToeEngineCommonObjs = "CommonObjects";
$RandToeEngineInter = "Interfaces";

Write-Output("");
Write-Output("");
Write-Host "Removing current files..." -ForegroundColor Yellow

Remove-Item "$OutputDir\*" -recurse

Write-Host "Copying files..." -ForegroundColor Yellow

Copy-Item  "$ConsoleRunner\*.cs" $OutputDir -recurse
Copy-Item  "$RandToeEngine\*.cs" $OutputDir -recurse

Copy-Item  "$RandToeEngine\$RandToeEngineBot\*.cs" "$OutputDir\" -recurse

Copy-Item  "$RandToeEngine\$RandToeEngineCommonObjs\*.cs" "$OutputDir\" -recurse

Copy-Item  "$RandToeEngine\$RandToeEngineInter\*.cs" "$OutputDir\" -recurse

Write-Host "Done!" -ForegroundColor Green;
Write-Output("");
Write-Output("");

if($doAll -ne $true)
{
    $input = Read-Host "To build press 'b', press anything else to exit"
}

if($input -eq "b" -or $doAll)
{
    cd $OutputDir
    Invoke-Expression "..\DoMonoBuild.bat"
    cd ..;

    Write-Output("");
    Write-Output("");
    Write-Host "Compile done!" -ForegroundColor Green;
    Write-Output("");
    Write-Output("");

    if($doAll -ne $true)
    {
        $input = Read-Host "To run press 'r', press anything else to exit"
    }

    if($input -eq "r" -or $doAll)
    {
        Write-Output("");
        Write-Output("");
        Write-Host "*************" -ForegroundColor Green;
        Write-Host "Running bot!" -ForegroundColor Green;
        Write-Host "*************" -ForegroundColor Green;
        Write-Output("");
        Write-Output("");
        Invoke-Expression " $OutputDir\BotManager.exe"
    }
}

Write-Output("");
Write-Output("");
Write-Output("All done, bye!!");
Write-Output("");
Write-Output("");