const officialServers = require("./official-servers.json");

const serversList = document.getElementById("serversList")


for (let i = 0; i < Object.keys(officialServers).length; i++) {
    const name = Object.keys(officialServers)[i];
    const icon = officialServers[name].icon
    serversList.innerHTML+= `<div class="item"><img src="./media/servericons/${icon}"><div class="name">${name}</div></div>`
}