param(
    [string]$PythonExecutable = "$env:USERPROFILE\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe",
    [string]$NodeExecutable = "$env:USERPROFILE\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\bin\node.exe",
    [string]$TectonicExecutable = 'tectonic',
    [string]$PlantUmlJar,
    [string]$JavaExecutable = 'java',
    [string]$RuntimeNodeModules = "$env:USERPROFILE\.cache\codex-runtimes\codex-primary-runtime\dependencies\node\node_modules",
    [string]$PresentationsSkillDir = "$env:USERPROFILE\.codex\plugins\cache\openai-primary-runtime\presentations\26.1013.11914\skills\presentations",
    [switch]$RigeneraDiagrammi,
    [switch]$SoloDocumento
)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    $prepareArgs = @('prepara_documentazione.py')
    if ($RigeneraDiagrammi) {
        if (!$PlantUmlJar) { throw 'Specificare -PlantUmlJar con -RigeneraDiagrammi.' }
        $prepareArgs += @('--render-diagrams','--java',$JavaExecutable,'--plantuml',$PlantUmlJar)
    }
    & $PythonExecutable @prepareArgs
    if ($LASTEXITCODE -ne 0) { throw 'Preparazione o controllo sorgenti/PNG non riuscito.' }
    & $TectonicExecutable --untrusted --keep-logs documentazione_finale.tex
    if ($LASTEXITCODE -ne 0) { throw 'Compilazione LaTeX non riuscita.' }
    if (!$SoloDocumento) {
        $savedModules = $env:ARTIFACT_NODE_MODULES
        $savedSkill = $env:PRESENTATIONS_SKILL_DIR
        $savedPython = $env:ARTIFACT_PYTHON
        try {
            $env:ARTIFACT_NODE_MODULES = $RuntimeNodeModules
            $env:PRESENTATIONS_SKILL_DIR = $PresentationsSkillDir
            $env:ARTIFACT_PYTHON = $PythonExecutable
            & $NodeExecutable presentazione.mjs
            if ($LASTEXITCODE -ne 0) { throw 'Generazione/validazione slide non riuscita.' }
        }
        finally {
            $env:ARTIFACT_NODE_MODULES = $savedModules
            $env:PRESENTATIONS_SKILL_DIR = $savedSkill
            $env:ARTIFACT_PYTHON = $savedPython
        }
    }
    Write-Host 'Artefatti rigenerati. Ricontrollare visivamente PDF e tutte le slide.'
    Write-Host 'La compilazione non esegue test applicativi e non chiude i punti aperti.'
}
finally { Pop-Location }
