const { app, BrowserWindow } = require('electron')

function createWindow () {
    const win = new BrowserWindow({
      width: 790,
      height: 500,
      webPreferences: {
        nodeIntegration: true
      }
    })
  
    win.loadFile('src/index.html')
    win.setMenu(null)
    win.setTitle("Horse Isle - Secret Land of Horses")
    win.setIcon("favicon.ico")
  }
  
  app.whenReady().then(createWindow)