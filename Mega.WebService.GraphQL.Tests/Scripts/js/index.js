var labelSrcBaseId = "LabelSrcBase";
var selectEnvId = "SelectEnv";
var selectSrcBaseId = "SelectSrcBase";
var selectDstBaseId = "SelectDstBase";
var selectProfileId = "SelectProfile";
var selectSynchronisation = "SelectSynchronisation";
var selectTestId = "SelectTest";
var textTestDescriptionId = "TextTestDescription";
var runningTest = false;

const testStatus = {
    running: 0,
    failure: 1,
    completed: 2
}

function GetSelectedJsonValue(id) {
    let selected = document.getElementById(id);
    let jsonSelected = selected.options[selected.selectedIndex].value;
    return JSON.parse(jsonSelected);
}

function GetSelectedStringValue(id) {
    let selected = document.getElementById(id);
    let strSelected = selected.hidden ? "" : selected.options[selected.selectedIndex].value;
    return strSelected;
}

function OnPageLoaded()
{
    OnSelectedTestChanged();
}

function OnSelectedTestChanged()
{
    //get items needed
    let selectedTest = GetSelectedJsonValue(selectTestId);
    let textTestDescription = document.getElementById(textTestDescriptionId);
    let labelSrcBase = document.getElementById(labelSrcBaseId);
    let selectSrcBase = document.getElementById(selectSrcBaseId);
    let hideSrcBase = selectedTest.hasOwnProperty("hideSourceRepository") ? selectedTest.hideSourceRepository : false;

    //update description
    textTestDescription.innerHTML = selectedTest.description;
    labelSrcBase.hidden = hideSrcBase;
    selectSrcBase.hidden = hideSrcBase;
}

function GenerateRepositoriesList(id, repositories)
{
    let dropDownList = document.getElementById(id);
    while (dropDownList.options.length > 0)
    {
        dropDownList.options.remove(0);
    }
    repositories.forEach(function (repository)
    {
        let option = document.createElement("option");
        option.value = repository.id;
        option.text = repository.name;
        dropDownList.options.add(option);
    });
    dropDownList.selectedIndex = 0;

}

function OnSelectedEnvChanged() {
    //get items needed
    let selectEnv = document.getElementById(selectEnvId);

    $.ajax({
        url: "/home/getrepos",
        type: "GET",
        data: "environmentId=" + selectEnv.value,
        success: function (response)
        {
            let repositories = JSON.parse(response);
            GenerateRepositoriesList(selectSrcBaseId, repositories);
            GenerateRepositoriesList(selectDstBaseId, repositories);
        },
        error: function (response)
        {
            alert(`Error trying to get repositories from environment ${selectEnv.name}.`)
        }
    });
}

function OnButtonRunClicked()
{
    if (CheckSessionDatas())
    {
        OnBeginTest();

        //get test to call
        let selectedTest = GetSelectedJsonValue(selectTestId);

        //test
        CallTest(selectedTest);
    }
}

function CheckSessionDatas()
{
    let repoSrcId = GetSelectedStringValue(selectSrcBaseId);
    let repoDstId = GetSelectedStringValue(selectDstBaseId);
    if (repoSrcId != "" && repoSrcId === repoDstId)
    {
        alert("Repositories source and target must be differents");
        return false;
    }
    return true;
}

function OnBeginTest()
{
    //get items to update
    let selectTest = document.getElementById(selectTestId);
    let imageState = document.getElementById("ImageState");
    let imageResult = document.getElementById("ImageResult");

    //update
    selectTest.disabled = true;
    document.getElementById(selectEnvId).disabled = true;
    document.getElementById(selectSrcBaseId).disabled = true;
    document.getElementById(selectDstBaseId).disabled = true;
    document.getElementById(selectProfileId).disabled = true;
    document.getElementById(selectSynchronisation).disabled = true;
    imageState.src = "Resources/Animations/running.gif";
    imageState.style.pointerEvents = "none";
    imageResult.hidden = true;
}

function UpdateMessages(result)
{
    document.getElementById("TestMessageDuration").innerHTML = result ? result.messageTime : "";
    document.getElementById("TestMessageCounts").innerHTML = result ? result.messageCounts : "";
    document.getElementById("TestMessageDetails").innerHTML = result ? result.messageDetails : "";
}

function UpdateMessagesError() {
    document.getElementById("TestMessageDetails").innerHTML = "error";
}

function CallTest(selectedTest)
{
    let testParams =
    {
        environmentId: GetSelectedStringValue(selectEnvId),
        repositoryIdSource: GetSelectedStringValue(selectSrcBaseId),
        repositoryIdTarget: GetSelectedStringValue(selectDstBaseId),
        profileId: GetSelectedStringValue(selectProfileId),
        synchronisation: GetSelectedStringValue(selectSynchronisation)
    }

    UpdateMessages(null);
    document.getElementById("TestCurrent").innerHTML = selectedTest.nameDisplay;
    $.ajax({
        url: "/home/runtestasync",
        type: "GET",
        data: "testName=" + selectedTest.name
            + "&testParams=" + JSON.stringify(testParams),
        success: function (response)
        {
            let result = JSON.parse(response);
            UpdateMessages(result);
            let success = (result.status == testStatus.completed);
            OnEndTest(success);
        },
        error: function ()
        {
            UpdateMessagesError();
            OnEndTest(false);
        }
    });
    runningTest = true;
    //CallProgression();
}

function CallProgression()
{
    $.ajax({
        url: "/home/getprogressionasync",
        type: "GET",
        success: function (response) {
            if (runningTest) {
                let result = JSON.parse(response);
                UpdateMessages(result);
                CallProgression();
            }
        },
        error: function () {
            UpdateMessagesError();
        }
    });
}

function OnEndTest(success)
{
    //get items to update
    let selectTest = document.getElementById(selectTestId);
    let imageState = document.getElementById("ImageState");
    let imageResult = document.getElementById("ImageResult");
    
    //update
    selectTest.disabled = false;
    document.getElementById(selectEnvId).disabled = false;
    document.getElementById(selectSrcBaseId).disabled = false;
    document.getElementById(selectDstBaseId).disabled = false;
    document.getElementById(selectProfileId).disabled = false;
    document.getElementById(selectSynchronisation).disabled = false;
    imageState.src = "Resources/Images/play.png";
    imageState.style.pointerEvents = "auto";
    imageResult.src = success ? "Resources/Images/success.png" : "Resources/Images/failure.png";
    imageResult.hidden = false;
    runningTest = false;
}
