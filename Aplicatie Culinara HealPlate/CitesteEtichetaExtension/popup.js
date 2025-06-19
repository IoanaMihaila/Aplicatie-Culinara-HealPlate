document.getElementById("btnTrimite").addEventListener("click", () => {
    chrome.tabs.query({ active: true, currentWindow: true }, ([tab]) => {
        chrome.tabs.sendMessage(tab.id, { actiune: "extrageIngrediente" }, (response) => {
            if (!response || !response.ingrediente || response.ingrediente.length === 0) {
                alert("Nu s-au putut extrage ingredientele din pagină.");
                return;
            }

            const ingrediente = response.ingrediente.map(i => i.trim());
            const ingredienteParam = encodeURIComponent(JSON.stringify(ingrediente));
            const url = `https://localhost:7159/ScanareIngrediente?ingrediente=${ingredienteParam}&sursa=extensie`;
            window.open(url, "_blank");
        });
    });
});
