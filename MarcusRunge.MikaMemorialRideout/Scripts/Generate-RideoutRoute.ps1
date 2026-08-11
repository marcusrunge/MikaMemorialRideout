[CmdletBinding()]
param(
    [Parameter()]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    if ([string]::IsNullOrWhiteSpace($PSScriptRoot)) {
        throw "Das Skriptverzeichnis konnte nicht ermittelt werden. Starte die Datei direkt mit -File."
    }

    $OutputPath = Join-Path -Path $PSScriptRoot -ChildPath "..\wwwroot\assets\mika-rideout-route.geojson"
}

$OutputPath = [System.IO.Path]::GetFullPath($OutputPath)

# Diese Punkte bilden die gewuenschte Acht ab. Fuer die Esterholzer Ausfahrt
# wird die OSRM-Geometrie nach der Berechnung deterministisch korrigiert.
$routePoints = @(
    [PSCustomObject]@{ Name = "Start Albrecht-Thaer-Gelaende"; Longitude = 10.5469518; Latitude = 52.9660785 },
    [PSCustomObject]@{ Name = "Elektro-Fundgrube"; Longitude = 10.5600803; Latitude = 52.9716882 },
    [PSCustomObject]@{ Name = "Zur Wipperau"; Longitude = 10.6054018; Latitude = 53.0030067 },
    [PSCustomObject]@{ Name = "Nordost-Wegpunkt"; Longitude = 10.6196660; Latitude = 52.9819037 },
    [PSCustomObject]@{ Name = "Pieperhoefen"; Longitude = 10.584848840701314; Latitude = 52.94856097401077 },
    [PSCustomObject]@{ Name = "Esterholzer Strasse nach der Abfahrt"; Longitude = 10.584757; Latitude = 52.950347 },
    [PSCustomObject]@{ Name = "Hochgraefestrasse"; Longitude = 10.5739326; Latitude = 52.9610085 },
    [PSCustomObject]@{ Name = "B4 Kreuzung suedlicher Bogen"; Longitude = 10.5412308; Latitude = 52.9451582 },
    [PSCustomObject]@{ Name = "K8 Klein Suestedt"; Longitude = 10.4909769; Latitude = 52.9293702 },
    [PSCustomObject]@{ Name = "Soltauer Strasse West"; Longitude = 10.543063; Latitude = 52.956788 },
    [PSCustomObject]@{ Name = "Soltauer Strasse Celler Strasse"; Longitude = 10.550168; Latitude = 52.956524 },
    [PSCustomObject]@{ Name = "Bohldamm"; Longitude = 10.548373; Latitude = 52.960179 },
    [PSCustomObject]@{ Name = "Ziel Albrecht-Thaer-Gelaende"; Longitude = 10.5469518; Latitude = 52.9660785 }
)

$coordinates = $routePoints | ForEach-Object {
    "{0},{1}" -f $_.Longitude.ToString([System.Globalization.CultureInfo]::InvariantCulture), $_.Latitude.ToString([System.Globalization.CultureInfo]::InvariantCulture)
}

$routeUri = "https://router.project-osrm.org/route/v1/driving/$($coordinates -join ';')?overview=full&geometries=geojson&steps=false"
$headers = @{ "User-Agent" = "MikaMemorialRideout-RoutePreparation/1.0"; "Accept" = "application/json" }

Write-Host "Routenberechnung fuer die Acht wird abgerufen ..."
$response = Invoke-RestMethod -Method Get -Uri $routeUri -Headers $headers

if ($response.code -ne "Ok" -or $null -eq $response.routes -or $response.routes.Count -eq 0) {
    throw "Der Routingdienst hat keine verwendbare Route geliefert. Status: $($response.code)"
}

$route = $response.routes[0]
$originalCoordinates = @($route.geometry.coordinates)

function Get-NearestCoordinateIndex {
    param(
        [Parameter(Mandatory)] [object[]]$Coordinates,
        [Parameter(Mandatory)] [double]$Longitude,
        [Parameter(Mandatory)] [double]$Latitude,
        [Parameter()] [int]$StartIndex = 0
    )

    $nearestIndex = -1
    $nearestDistance = [double]::MaxValue

    for ($index = $StartIndex; $index -lt $Coordinates.Count; $index++) {
        $longitudeDifference = [double]$Coordinates[$index][0] - $Longitude
        $latitudeDifference = [double]$Coordinates[$index][1] - $Latitude
        $distance = ($longitudeDifference * $longitudeDifference) + ($latitudeDifference * $latitudeDifference)

        if ($distance -lt $nearestDistance) {
            $nearestDistance = $distance
            $nearestIndex = $index
        }
    }

    if ($nearestIndex -lt 0) {
        throw "Fuer die Geometriebereinigung wurde kein passender Routenpunkt gefunden."
    }

    return $nearestIndex
}

# OSRM ordnet die Abfahrt teilweise der falschen Richtungsfahrbahn zu und erzeugt
# dadurch einen sinnlosen Stich nach Sueden mit anschliessender Rueckfahrt.
# Der gesamte Abschnitt zwischen Pieperhoefen und der eindeutig erkannten
# Esterholzer Strasse wird deshalb durch die reale Ausfahrtsrampe ersetzt.
$pieperhoefenIndex = Get-NearestCoordinateIndex -Coordinates $originalCoordinates -Longitude 10.584848840701314 -Latitude 52.94856097401077
$esterholzerIndex = Get-NearestCoordinateIndex -Coordinates $originalCoordinates -Longitude 10.584757 -Latitude 52.950347 -StartIndex ($pieperhoefenIndex + 1)

if ($esterholzerIndex -le $pieperhoefenIndex) {
    throw "Die Esterholzer Strasse liegt in der berechneten Geometrie nicht hinter Pieperhoefen."
}

$correctedCoordinates = [System.Collections.Generic.List[object]]::new()

for ($index = 0; $index -le $pieperhoefenIndex; $index++) {
    $correctedCoordinates.Add($originalCoordinates[$index])
}

# Positive, vom Veranstalter kontrollierte Geometriepunkte der gewuenschten Ausfahrt.
$correctedCoordinates.Add(@(10.583946, 52.948403))
$correctedCoordinates.Add(@(10.584757, 52.950347))

for ($index = $esterholzerIndex + 1; $index -lt $originalCoordinates.Count; $index++) {
    $correctedCoordinates.Add($originalCoordinates[$index])
}

$route.geometry.coordinates = $correctedCoordinates.ToArray()
$removedCoordinateCount = $esterholzerIndex - $pieperhoefenIndex - 1

$featureCollection = [ordered]@{
    type = "FeatureCollection"
    name = "Mika Memorial Rideout - Route als Acht"
    properties = [ordered]@{
        source = "OSRM auf Basis von OpenStreetMap-Daten mit lokaler Geometriekorrektur"
        generatedAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
        distanceMeters = [Math]::Round([double]$route.distance)
        durationSeconds = [Math]::Round([double]$route.duration)
        reviewRequired = $true
        routeShape = "figure-eight"
        geometryCorrection = [ordered]@{
            name = "L270 Esterholzer Strasse"
            method = "replace-osrm-detour"
            removedCoordinateCount = $removedCoordinateCount
            ramp = [ordered]@{ longitude = 10.583946; latitude = 52.948403 }
            esterholzerStreet = [ordered]@{ longitude = 10.584757; latitude = 52.950347 }
            excludedSouthContinuation = [ordered]@{ longitude = 10.580840; latitude = 52.946050 }
        }
        controlPoints = @($routePoints | ForEach-Object { [ordered]@{ name = $_.Name; longitude = $_.Longitude; latitude = $_.Latitude } })
    }
    features = @(
        [ordered]@{
            type = "Feature"
            properties = [ordered]@{ name = "Mika Memorial Rideout"; stroke = "#3b16d9"; strokeWidth = 5; strokeOpacity = 1 }
            geometry = $route.geometry
        }
    )
}

$outputDirectory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
$featureCollection | ConvertTo-Json -Depth 100 | Set-Content -Path $OutputPath -Encoding utf8

$distanceKilometers = [Math]::Round([double]$route.distance / 1000, 1)
$duration = [TimeSpan]::FromSeconds([double]$route.duration)

Write-Host "Route erstellt: $OutputPath"
Write-Host "Entfernung laut Routingdienst: $distanceKilometers km"
Write-Host "Routingzeit laut Routingdienst: $($duration.ToString('hh\:mm')) Stunden"
Write-Host "Entfernte Koordinaten des falschen Uhlenring-Stichs: $removedCoordinateCount"
Write-Host "Geometrie fuehrt jetzt direkt ueber die L270-Ausfahrtsrampe in die Esterholzer Strasse."
Write-Warning "Entfernung und Zeit stammen noch aus der OSRM-Ausgangsroute. Bitte die sichtbare Linie vor der Veroeffentlichung abschliessend pruefen."
