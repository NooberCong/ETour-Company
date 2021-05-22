// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function toggleNav(btn) {
    if (btn.classList.contains("is-active")) {
        document.getElementById("sideNav").style.width = "0";
        btn.classList.remove("is-active");
    } else {
        btn.classList.add("is-active");
        document.getElementById("sideNav").style.width = "calc(100vw / 3)";
    }
}

function onResizeRestoreSidebar() {
    if (document.documentElement.clientWidth >= 768 && document.getElementById("sideNav").style.width == "0px") {
        document.getElementById("sideNav").style.width = null;
    }
}

function closeSideNavOnClickOutside(e) {
    let btn = document.getElementById("burgerMenu");
    if (document.documentElement.clientWidth < 768 && btn.classList.contains("is-active")) {
        btn.click();
    }
}