var url : String;

	var source: AudioSource;
	var Presource : AudioSource;
	var volumeMax = 1.0;
	var volumeMin = 0.0;
	var volumeMore = 0.1;

	function Start () {
		var www = new WWW(url);
		source = GetComponent.<AudioSource>();
		source.clip = www.GetAudioClip();
	
	}

	function Update () {

		

		if(!source.isPlaying && source.clip.loadState){
			source.Play();
			}
	 else {

     Debug.Log("waiting - isplaying : " + source.isPlaying + " loadState : " + source.clip.loadState);

 }
 	//
 	if(!source.isPlaying){
 		Presource.Play();
 		} else{
 		Presource.Stop();
 	} 
 	// MUTE
 	if(Input.GetKeyDown(KeyCode.M)) {
		 	if(source.mute)
				source.mute = false;
			else
				source.mute = true;
		}

 	//VOLUME
 	if(source.volume < volumeMin){

 		source.volume = volumeMin;
 		}
	//
 	if(source.volume > volumeMax){

 	source.volume = volumeMax;
 	}

 	if(Input.GetKeyDown(KeyCode.KeypadPlus)){
 		source.volume = source.volume + volumeMore;

 	}

 	  if(Input.GetKeyDown(KeyCode.KeypadMinus)){
 		source.volume = source.volume - volumeMore;

 	}

 }