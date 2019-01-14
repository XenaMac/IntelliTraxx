var polyPublish = new function(){
	
	//foreign accessible publish function
	this.publish = function postPolygon(polyData){
		var poly = parseObject(polyData);
		publishPolygon(poly);
	}
	
	function parseObject(polyData){
		var _json = $.parseJSON(polyData);
		var _oldJSON = $('#mapData').val();
		if(_oldJSON != ''){
			var _origData = $.parseJSON($('#mapData').val());
			_json.MapID = _origData.MapID;
		}
		var latLngList = [];
		var latLng = _json.overlays[0].paths[0];
		var iLength = latLng.length;
		for(var i=0;i< latLng.length;i++){
			var _latLngObject = {
				Lat:latLng[i].lat,
				Lon:latLng[i].lng,
				Alt:0
			};
			latLngList.push(_latLngObject);
		}
		//place holder for when we have real IDs from the service
		var cID = '00000000-0000-0000-0000-000000000000';
		var _polyObject = {
			poly: {
				geoFenceID: _json.MapID,
				polyID: _json.MapID,
				notes: _json.overlays[0].content,
				polyName: _json.overlays[0].title,
				geoFence: latLngList,
				actionIn: 'something',
				actionOut: 'something'
			}
		};
		return _polyObject;
	}
	
	function generateUUID(cID) {
		if(cID != '00000000-0000-0000-0000-000000000000'){
			return cID;
		}
		var d = new Date().getTime();
		var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
			var r = (d + Math.random()*16)%16 | 0;
			d = Math.floor(d/16);
			return (c=='x' ? r : (r&0x3|0x8)).toString(16);
			});
		return uuid;
};

	
	function publishPolygon(polyData){
		try{
			var srvURL = url + 'addPolygon';
			var polyString = JSON.stringify(polyData);
			$.ajax({
				type: "POST",
				dataType: "json",
				url: srvURL,
				data: polyString,
				contentType: "application/json; charset=utf-8",
				success: publishPolygonSuccess,
				error: publishPolygonError
			});
		}
		catch(err){
			alert(err.message);
		}
	}
	
	function publishPolygonSuccess(result){
		//publish worked, raise an alert to the let the user know it's all good
		alert('Polygon successfully published to the service');
	}
	
	function publishPolygonError(error){
		//something bad happened, let the user know their polygon didn't go
		alert('Publishing failed\r\n' + error);
	}
}
