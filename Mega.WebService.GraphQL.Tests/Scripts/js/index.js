function GetSelectedTest()
{
    let selectTest = document.getElementById("SelectTest");
    let jsonSelectedTest = selectTest.options[selectTest.selectedIndex].value;
    return JSON.parse(jsonSelectedTest);
}

function OnPageLoaded()
{
    OnSelectedTestChanged();
}

function OnSelectedTestChanged()
{
    //get items needed
    let selectedTest = GetSelectedTest();
    let textTestDescription = document.getElementById("TextTestDescription");

    //update description
    textTestDescription.innerHTML = selectedTest.description;
}

function OnButtonRunClicked()
{
    OnBeginTest();

    //get test to call
    let selectedTest = GetSelectedTest();

    //test
    CallTest(selectedTest);
}

function OnBeginTest()
{
    //get items to update
    let selectTest = document.getElementById("SelectTest");
    let imageState = document.getElementById("ImageState");

    //update
    selectTest.disabled = true;
    document.getElementById("InputEnvironmentId").disabled = true;
    document.getElementById("InputRepositoryIdSource").disabled = true;
    document.getElementById("InputRepositoryIdTarget").disabled = true;
    document.getElementById("InputProfileId").disabled = true;
    imageState.src = "Resources/Animations/running.gif";
    imageState.style.pointerEvents = "none";
}

function CallTest(selectedTest)
{
    let testParams =
    {
        environmentId: document.getElementById("InputEnvironmentId").value,
        repositoryIdSource: document.getElementById("InputRepositoryIdSource").value,
        repositoryIdTarget: document.getElementById("InputRepositoryIdTarget").value,
        profileId: document.getElementById("InputProfileId").value
    }

    $.ajax({
        url: "/home/runtestasync",
        type: "GET",
        data: "testName=" + selectedTest.name
            + "&testParams=" + JSON.stringify(testParams),
        success: function (response) {
            let result = JSON.parse(response);
            document.getElementById("TestFinished").innerHTML = selectedTest.nameDisplay;
            document.getElementById("TestMessageDuration").innerHTML = result.MessageTime;
            document.getElementById("TestMessageCounts").innerHTML = result.MessageCounts;
            document.getElementById("TestMessageDetails").innerHTML = result.MessageDetails;
            OnEndTest();
        },
        error: function (response) {
            document.getElementById("TestFinished").innerHTML = selectedTest.nameDisplay;
            document.getElementById("TestMessageDuration").innerHTML = "";
            document.getElementById("TestMessageCounts").innerHTML = "";
            document.getElementById("TestMessageDetails").innerHTML = response.statusText;
            OnEndTest();
        }
    });
}

function OnEndTest()
{
    //get items to update
    let selectTest = document.getElementById("SelectTest");
    let imageState = document.getElementById("ImageState");

    //update
    selectTest.disabled = false;
    document.getElementById("InputEnvironmentId").disabled = false;
    document.getElementById("InputRepositoryIdSource").disabled = false;
    document.getElementById("InputRepositoryIdTarget").disabled = false;
    document.getElementById("InputProfileId").disabled = false;
    imageState.src = "Resources/Images/play.png";
    imageState.style.pointerEvents = "auto";
}
