window.plmUpload = window.plmUpload || {};

window.plmUpload.uploadAndBind = async function (config) {
    const input = document.getElementById(config.fileInputId);
    const urlInput = document.getElementById(config.urlInputId);
    const nameInput = document.getElementById(config.nameInputId);
    const statusElement = document.getElementById(config.statusElementId);

    if (!input || !urlInput || !nameInput) {
        return;
    }

    if (!input.files || input.files.length === 0) {
        if (statusElement) statusElement.textContent = "Please choose a file first";
        return;
    }

    if (statusElement) statusElement.textContent = "Uploading...";

    const file = input.files[0];
    const formData = new FormData();
    formData.append("file", file);

    try {
        const response = await fetch(`/api/attachments/upload/${encodeURIComponent(config.scope)}`, {
            method: "POST",
            body: formData,
            credentials: "same-origin"
        });

        let payload = null;
        try {
            payload = await response.json();
        } catch {
            payload = null;
        }

        if (!response.ok || !payload || !payload.url) {
            const message = payload?.message || "Upload failed.";
            throw new Error(message);
        }

        urlInput.value = payload.url;
        urlInput.dispatchEvent(new Event("change", { bubbles: true }));

        nameInput.value = payload.originalFileName || file.name;
        nameInput.dispatchEvent(new Event("change", { bubbles: true }));

        input.value = "";
        if (statusElement) statusElement.textContent = "Uploaded";
    } catch (error) {
        if (statusElement) {
            statusElement.textContent = error?.message || "Upload failed";
        }
    }
};
