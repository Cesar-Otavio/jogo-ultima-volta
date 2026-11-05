# Recoloca no projeto os sprites da CraftPix, que não vão para o GitHub
# (a licença gratuita não permite redistribuir os arquivos de arte).
#
# Rode ANTES de abrir o projeto na Unity, para os sprites manterem as
# ligações com as cenas (os arquivos .meta já estão no repositório).
#
# Uso, no PowerShell, dentro da pasta do projeto:
#   powershell -ExecutionPolicy Bypass -File tools\importar_sprites_craftpix.ps1 -Pasta "C:\caminho\dos\pacotes"
#
# A pasta deve conter os dois pacotes gratuitos já extraídos:
#   https://craftpix.net/freebies/free-racing-game-kit/
#   https://craftpix.net/freebies/free-race-track-2d-game-tile-set/

param([Parameter(Mandatory = $true)][string]$Pasta)

$projeto = Split-Path -Parent $PSScriptRoot
$lista = Join-Path $projeto "Assets\Editor\CraftPix\sprites_craftpix.txt"

# Indexa os PNGs dos pacotes pelo nome do arquivo.
$porNome = @{}
Get-ChildItem $Pasta -Recurse -Filter *.png | ForEach-Object {
    if (-not $porNome.ContainsKey($_.Name)) { $porNome[$_.Name] = @() }
    $porNome[$_.Name] += $_.FullName
}

$copiados = 0; $existentes = 0; $faltando = @()
foreach ($linha in Get-Content $lista -Encoding UTF8) {
    if ($linha.StartsWith("#") -or $linha.Trim() -eq "") { continue }
    $destino, $nome, $hash = $linha.Split("|")
    $alvo = Join-Path $projeto $destino
    if (Test-Path $alvo) { $existentes++; continue }

    # Vários arquivos podem ter o mesmo nome: vale o de mesmo conteúdo.
    $origem = $null
    foreach ($candidato in $porNome[$nome]) {
        if ((Get-FileHash $candidato -Algorithm SHA256).Hash -eq $hash) { $origem = $candidato; break }
    }
    if ($null -eq $origem) { $faltando += "$destino ($nome)"; continue }

    New-Item -ItemType Directory -Force (Split-Path -Parent $alvo) | Out-Null
    Copy-Item $origem $alvo
    $copiados++
}

Write-Host "$copiados sprites copiados, $existentes já existiam."
if ($faltando.Count -gt 0) {
    Write-Host "Não encontrados ($($faltando.Count)):"
    $faltando | ForEach-Object { Write-Host "  $_" }
}
