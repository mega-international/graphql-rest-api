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
    if (selected == null || selected.hidden) {
        return "";
    }
    return selected.options[selected.selectedIndex].value;
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

function enableElement(elementName) {
    let element = document.getElementById(elementName);
    if (element != null) {
        element.disabled = false;
    }
}

function disableElement(elementName) {
    let element = document.getElementById(elementName);
    if (element != null) {
        element.disabled = true;
    }
}

function OnBeginTest()
{
    //get items to update
    let selectTest = document.getElementById(selectTestId);
    let imageState = document.getElementById("ImageState");
    let imageResult = document.getElementById("ImageResult");

    //update
    selectTest.disabled = true;
    disableElement(selectEnvId);
    disableElement(selectSrcBaseId);
    disableElement(selectDstBaseId);
    disableElement(selectProfileId);
    disableElement(selectSynchronisation);
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

function GetTestParametersStr()
{
    let hasMode = document.getElementById("HASMode").value;
    let src, dst;
    if (hasMode) {
        src = {
            apiKey: GetSelectedStringValue(selectSrcBaseId)
        };
        dst = {
            apiKey: GetSelectedStringValue(selectDstBaseId)
        };
    }
    else {
        src = {
            environmentId: GetSelectedStringValue(selectEnvId),
            repositoryId: GetSelectedStringValue(selectSrcBaseId),
            profileId: GetSelectedStringValue(selectProfileId)
        }
        dst = {
            environmentId: GetSelectedStringValue(selectEnvId),
            repositoryId: GetSelectedStringValue(selectDstBaseId),
            profileId: GetSelectedStringValue(selectProfileId)
        }
    }
    let testParams = {
        source: src,
        destination: dst,
        synchronisation: GetSelectedStringValue(selectSynchronisation)
    }
    return JSON.stringify(testParams);

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

    let testParamsStr = GetTestParametersStr();

    UpdateMessages(null);
    document.getElementById("TestCurrent").innerHTML = selectedTest.nameDisplay;
    $.ajax({
        url: "/home/runtestasync",
        type: "GET",
        data: "testName=" + selectedTest.name
            + "&testParams=" + testParamsStr,
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
    enableElement(selectEnvId);
    enableElement(selectSrcBaseId);
    enableElement(selectDstBaseId);
    enableElement(selectProfileId);
    enableElement(selectSynchronisation);
    imageState.src = "Resources/Images/play.png";
    imageState.style.pointerEvents = "auto";
    imageResult.src = success ? "Resources/Images/success.png" : "Resources/Images/failure.png";
    imageResult.hidden = false;
    runningTest = false;
}
