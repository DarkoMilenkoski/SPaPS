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


    /*let form = document.querySelector("form");
    form.addEventListener("submit", function () {
        e.preventDefault();
        console.log("test");
    })*/

    /*form.onsubmit = function validateForm() {
        let formInputs = document.querySelector("form").querySelectorAll("input, select");
        formInputs.forEach(input => {
            if (input.classList.contains("required") && input == null)
                return false;
        });
        return true;

        let form = document.forms[0];
        let role = form.Role.value;
        if (role == "Изведувач") {
            if (form.NoOfEmployees.value == null || form.DateOfEstablishment.value == null || form.Activities.value == null)
                return false;
        }
        return true;
    };*/
});