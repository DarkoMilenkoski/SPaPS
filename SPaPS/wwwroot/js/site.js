$(document).ready(function () {
    $('.dropdown-select').select2({
        allowClear: true,
        theme: 'bootstrap-5'
    });

    let roleInput = document.querySelector("#Role");
    let noOfEmpDiv = document.querySelector(".NoOfEmployees");
    let dateOfEstDiv = document.querySelector(".DateOfEstablishment");
    let activitiesDiv = document.querySelector(".Activities");

    let noOfEmpInput = document.querySelector("#NoOfEmployees");
    let dateOfEstInput = document.querySelector("#DateOfEstablishment");
    let activitiesInput = document.querySelector("#Activities");

    if (roleInput == "Изведувач") {
        noOfEmpDiv.classList.remove("d-none");
        dateOfEstDiv.classList.remove("d-none");
        activitiesDiv.classList.remove("d-none");
    }

    if (roleInput != null) {
        roleInput.addEventListener("change", function () {
            if (roleInput.value == "Изведувач") {
                noOfEmpDiv.classList.remove("d-none");
                dateOfEstDiv.classList.remove("d-none");
                activitiesDiv.classList.remove("d-none");
            }
            else {
                noOfEmpDiv.classList.add("d-none");
                dateOfEstDiv.classList.add("d-none");
                activitiesDiv.classList.add("d-none");

                noOfEmpInput.value = null;
                dateOfEstInput.value = null;
                activitiesInput.value = null;
            }
        })
    }


    let allForms = document.querySelectorAll("form");
    let registerChangeForm;
    allForms.forEach(x => {
        if (x.classList.contains("registerChange")) registerChangeForm = x;
    })

    if (registerChangeForm != undefined) {
        registerChangeForm.addEventListener("submit", function (e) {
            let required = document.querySelectorAll(".required")
            required.forEach(x => {
                if (x.value == "" && roleInput.value == "Изведувач") {
                    x.classList.add("is-invalid");
                    e.preventDefault();
                    console.log("please fill all inputs");
                }
            })
            let inputDate = new Date(dateOfEstInput.value);
            let currentDate = new Date();
            if (inputDate > currentDate) {
                dateOfEstInput.classList.add("is-invalid");
                e.preventDefault();
                console.log("Date cant be in the future");
            }
        })
    }

    let serviceInput = document.querySelector("#ServiceId");
    if (serviceInput) {
        let buildingType = document.querySelector(".BuildingTypeId");
        let buildingSize = document.querySelector(".BuildingSize");
        let color = document.querySelector(".Color");
        let noOfWindows = document.querySelector(".NoOfWindows");
        let noOfDoors = document.querySelector(".NoOfDoors");

        serviceInput.addEventListener("change", function () {

            buildingType.classList.add("d-none");
            buildingSize.classList.add("d-none");
            color.classList.add("d-none");
            noOfWindows.classList.add("d-none");
            noOfDoors.classList.add("d-none");

            let serviceId = serviceInput.value;

            if (serviceId == null) {
                return;
            }

            if (serviceId == 1) {
                buildingType.classList.remove("d-none");
                noOfWindows.classList.remove("d-none");
                noOfDoors.classList.remove("d-none");
            }

            if (serviceId == 2) {
                buildingType.classList.remove("d-none");
                buildingSize.classList.remove("d-none");
                color.classList.remove("d-none");
            }

            if (serviceId == 3) {
                buildingType.classList.remove("d-none");
                buildingSize.classList.remove("d-none");
            }
        });
    }

});