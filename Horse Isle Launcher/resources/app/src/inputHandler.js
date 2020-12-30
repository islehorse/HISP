const util = require('util');
    const exec = util.promisify(require('child_process').exec)

const urlInput = document.getElementById("urlInput")
const ipInput = document.getElementById("ipInput")
const portInput = document.getElementById("portInput")
const enterButton = document.getElementById("enterButton")
const settingsButton = document.getElementById("settingsButton")

let showServerList = false;

const lastDetails = JSON.parse(localStorage.getItem("lastDetails"));

if (lastDetails) {
    urlInput.value = lastDetails.url;
    ipInput.value = lastDetails.ip;
    portInput.value = lastDetails.port;
}

enterButton.addEventListener("click", async () => {
    const url = urlInput.value;
    const ip = ipInput.value;
    const port = portInput.value;

    localStorage.setItem("lastDetails", JSON.stringify({
        url,
        ip,
        port
    }))

    runCommand(`hirunner.exe "${url}?SERVER=${ip}&PORT=${port}"`)
    setTimeout(() => {
        window.close()
    }, 500);
})


async function runCommand(command) {
    const { stdout, stderr } = await exec(command);
    if(DEBUG_MODE)
    {
      console.log('stdout:', stdout);
      console.log('stderr:', stderr);
    }
  }


serversList.addEventListener("click", (event) => {
    if (!event.target.closest(".item")) return;
    const name = event.target.innerText;
    const fullUrl = new URL(officialServers[name].url);
    const url = fullUrl.protocol + "//" + fullUrl.host + fullUrl.pathname;
    const ip = fullUrl.searchParams.get("SERVER");
    const port = fullUrl.searchParams.get("PORT");


    urlInput.value = url;
    ipInput.value = ip;
    portInput.value = port;
})

settingsButton.addEventListener("click", () => {
    serversList.style.display = showServerList ? "none" : "block";
    showServerList = !showServerList;
})