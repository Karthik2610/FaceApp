'use strict';

const videoElement = document.querySelector('video');
const videoSelect = document.querySelector('select#videoSource');
const selectors = [videoSelect];
var setDefaultTimer;

function gotDevices(deviceInfos) {
    const values = selectors.map(select => select.value);
    selectors.forEach(select => {
        while (select.firstChild) {
            select.removeChild(select.firstChild);
        }
    });
    for (let i = 0; i !== deviceInfos.length; ++i) {
        const deviceInfo = deviceInfos[i];
        const option = document.createElement('option');
        //const option1 = document.createElement('option');
        option.value = deviceInfo.deviceId;
        if (deviceInfo.kind === 'videoinput') {
            option.text = deviceInfo.label || `camera ${videoSelect.length + 1}`;
            videoSelect.appendChild(option);
            //option1.value = "Back";
            //option1.text = "Back Camera";
            //videoSelect.appendChild(option1);
        } else {
            console.log('Some other kind of source/device: ', deviceInfo);
        }
    }
    selectors.forEach((select, selectorIndex) => {
        if (Array.prototype.slice.call(select.childNodes).some(n => n.value === values[selectorIndex])) {
            select.value = values[selectorIndex];
        }
    });
}

navigator.mediaDevices.enumerateDevices().then(gotDevices).catch(handleError);

function gotStream(stream) {
    // window.stream = stream; // make stream available to console
    videoElement.srcObject = stream;
    // Refresh button list in case labels have become available
    return navigator.mediaDevices.enumerateDevices();
}

function handleError(error) {
    console.log('navigator.MediaDevices.getUserMedia error: ', error.message, error.name);
}
function selectDefaultBackCamera() {
    setDefaultTimer = window.setTimeout(function () {
        $("#videoSource > option").each(function () {
            //alert(this.text.indexOf("Back"));
            if (this.text.indexOf("Back") != -1) {
                this.selected = true;
                $("#videoSource").trigger("change");
                window.clearTimeout(setDefaultTimer);
                // alert(this.text + ', ' + this.value);
            }
        });
    }, 1500);
}
function startStreaming() {
    //if (window.stream) {
    //  window.stream.getTracks().forEach(track => {
    //    track.stop();
    //  });
    //}    
    const videoSource = videoSelect.value;
    const constraints = {
        video: { deviceId: videoSource ? { exact: videoSource } : undefined }
    };
    navigator.mediaDevices.getUserMedia(constraints).then(gotStream).then(gotDevices).catch(handleError);
}

videoSelect.onchange = startStreaming;
