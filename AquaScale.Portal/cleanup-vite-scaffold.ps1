# Run from inside AquaScale.FieldWorker\ or AquaScale.Portal\
# Usage:  powershell -ExecutionPolicy Bypass -File cleanup-vite-scaffold.ps1

# Delete Vite boilerplate
Remove-Item -Force -ErrorAction SilentlyContinue `
    src\App.css, src\assets\hero.png, src\assets\react.svg, src\assets\vite.svg

# Mirror the pages/components/api/context structure from Back Office
New-Item -ItemType Directory -Force -Path `
    src\api, src\pages, src\components\layout, src\components\ui, src\context | Out-Null
New-Item -ItemType File -Force -Path `
    src\components\layout\.gitkeep, src\components\ui\.gitkeep, src\context\.gitkeep, src\pages\.gitkeep | Out-Null

# Minimal placeholder App.jsx (swap for real content once you build the first page)
@'
function App() {
  return (
    <div>
      <h1>App placeholder</h1>
    </div>
  )
}

export default App
'@ | Set-Content -Path src\App.jsx -Encoding utf8

# Trimmed reset — same #root width-cap fix applied to Back Office
@'
:root {
  color-scheme: light;
  font-synthesis: none;
  text-rendering: optimizeLegibility;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
}

* {
  box-sizing: border-box;
}

body {
  margin: 0;
}

#root {
  min-height: 100svh;
}

h1,
h2,
h3,
p {
  margin: 0;
}
'@ | Set-Content -Path src\index.css -Encoding utf8

Write-Host "Done. Structure now matches Back Office's src/ layout."
