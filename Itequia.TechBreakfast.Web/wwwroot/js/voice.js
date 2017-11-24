var audio_context;
var recorder;

$(window).load(function() {
	try {
		// webkit shim
		window.AudioContext = window.AudioContext || window.webkitAudioContext;
		navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia;
		window.URL = window.URL || window.webkitURL;

		audio_context = new AudioContext;
	} catch (e) {
		alert('No web audio support in this browser!');
	}
	

	navigator.getUserMedia({ audio: true }, startUserMedia, function (e) {
	});
});

function startUserMedia(stream) {
	var input = audio_context.createMediaStreamSource(stream);
	// Uncomment if you want the audio to feedback directly
	//input.connect(audio_context.destination);
	//__log('Input connected to audio context destination.');
	

	recorder = new Recorder(input);
}

function toggleRecord(button) {
	$('.recognition-result').addClass('hidden');
	$('#success-recognition').text("");

	if ($(button).data('recording'))
	{
		recorder && recorder.stop();
		uploadAudioData(button);
		
		recorder.clear();
	}
	else {
		recorder && recorder.record();

		$(button).find('span').text('Stop recording')
		$(button).addClass('active')
		$(button).data('recording', true);
	}
}

function uploadAudioData(button) {
	$(button).find('span').text('')
	$(button).find('i').removeClass('hidden')
	recorder && recorder.exportWAV(function (blob) {

		var fd = new FormData();
		fd.append('data', blob);
		
		$.ajax({
			type: "POST",
			url: "/voice",
			data: fd,
			beforeSend: function (xhr) {
				xhr.setRequestHeader("XSRF-TOKEN",
					$('input:hidden[name="__RequestVerificationToken"]').val());
			},
			contentType: false,
			processData: false,
			success: function (response) {

				var json = JSON.parse(response);

				if (json["RecognitionStatus"] == "Success")
				{
					$('#success-recognition').text(json["DisplayText"]);
					$('#success-recognition').removeClass("hidden");
				}
				else
				{
					$('#fail-recognition').removeClass("hidden");
				}

				$(button).find('i').addClass('hidden')
				$(button).removeClass('active')
				$(button).find('span').text('Start recording')
				$(button).data('recording', false);
			},
			failure: function (response) {
				alert(response);
			}
		});			
	});
}