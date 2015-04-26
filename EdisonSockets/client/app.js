/// <reference path="jquery-1.6.4.js" />

//$.connection.hub.url = "http://localhost:8088/signalr/hubs";
$.connection.hub.url = "http://#ip:8088/signalr";

var proxy = $.connection.MyHub;

var canvas = document.getElementById('canvas-video');
var context = canvas.getContext('2d');
var img = new Image();
var timer = document.getElementById("timer");


// show loading notice
context.fillStyle = '#333';
context.fillText('Loading...', canvas.width/2-30, canvas.height/3);

proxy.client.whatTimeIsIt = function (data) {
    timer.innerText = data;
};

proxy.client.frame = function (data) {
  // Reference: http://stackoverflow.com/questions/24107378/socket-io-began-to-support-binary-stream-from-1-0-is-there-a-complete-example-e/24124966#24124966
  //var uint8Arr = new Uint8Array(data);
  //var str = String.fromCharCode.apply(null, uint8Arr);
    //str = data;
  //var base64String = btoa(str);
    var base64String = data;

  img.onload = function () {
    context.drawImage(this, 0, 0, canvas.width, canvas.height);
  };
  img.src = 'data:image/png;base64,' + base64String;
};

$.connection.hub.start().done(function() {
    console.log('Connected ' + $.connection.hub.id);
});

jQuery("#closeButton").click(function() {
    $.connection.hub.stop();
    context.fillStyle = '#333';
    context.fillRect(0, 0, canvas.width, canvas.height);
    //context.drawImage(this, 0, 0, canvas.width, canvas.height);
});