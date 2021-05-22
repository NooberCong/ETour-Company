function handleFiles(files, containerId) {
    let container = document.getElementById(containerId);
    container.innerHTML = '';


    for (let file of files) {
        let reader = new FileReader()
        reader.readAsDataURL(file)
        reader.onloadend = function () {
            let previewImg = document.createElement("img")
            previewImg.classList.add("img-fluid", "mr-2", "mb-2");
            previewImg.style.width = "13rem";
            previewImg.src = reader.result
            container.appendChild(previewImg)
        }
    }
}