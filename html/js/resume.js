
function setEmployment() {
    let totalExperience = 0;
    let netExperience = 0;
    let crmExperience = 0;
    const array = ["dmi", "ags", "indiabulls", "minerva"];
    const employment = {
        indiabulls: {
            startDate: "2016-08-02",
            endDate: "2021-03-31",
            netDeveloper: true,
            crmDeveloper: true,
        },
        minerva: {
            startDate: "2015-05-02",
            endDate: "2016-07-31",
            netDeveloper: true,
            crmDeveloper: null,
        },
        dmi: {
            startDate: "2021-06-21",
            endDate: null,
            crmDeveloper: true,
            netDeveloper: null,
        },
        ags: {
            startDate: "2021-04-01",
            endDate: "2021-06-20",
            netDeveloper: null,
            crmDeveloper: true,
        },
    };
    array.forEach((key) => {
        const company = employment[key];
        const htmlElement = document.getElementById(key);
        if (htmlElement) {
            if (!company.endDate) {
                const today = new Date();
                const htmlCreatedOnElement = document.getElementById("createdOn");
                if (htmlCreatedOnElement) {
                    htmlCreatedOnElement.innerHTML = moment(today).format("LL");
                }
                const htmlElementAge = document.getElementById("age");
                const htmlElementDob = document.getElementById("dob");
                company.endDate = new Date(
                    today.getFullYear(),
                    today.getMonth(),
                    today.getDate()
                );
                htmlElementAge.innerText = moment(company.endDate).diff(
                    htmlElementDob.innerText,
                    "years"
                );
                htmlElement.innerHTML = `${moment.preciseDiff(
                    company.startDate,
                    company.endDate
                )}<br/>(${company.startDate} ~ Present)`;
            } else {
                htmlElement.innerHTML = `${moment.preciseDiff(
                    company.startDate,
                    company.endDate
                )}<br/>(${company.startDate} ~ ${company.endDate})`;
            }
            totalExperience += moment(company.endDate).diff(
                company.startDate,
                "years",
                true
            );
            if (company.crmDeveloper) {
                crmExperience += moment(company.endDate).diff(
                    company.startDate,
                    "years",
                    true
                );
            }
            if (company.netDeveloper) {
                netExperience += moment(company.endDate).diff(
                    company.startDate,
                    "years",
                    true
                );
            }
        }
    });
    document.getElementById("totalExperience").innerHTML = `${totalExperience.toFixed(1)} years`;
    document.getElementById("crmExperience").innerHTML = `${crmExperience.toFixed(1)} years`;
    document.getElementById("dotNetExperience").innerHTML = `${netExperience.toFixed(1)} years`;

}