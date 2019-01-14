var polyGet = new function(){
	
	this.getFences = function getFenceNames(){
		var srvURL = url + 'getFenceNames';
		var _selectCode = '';
		$.ajax({
            type: "GET",
            dataType: "json",
            url: srvURL,
            contentType: "application/json; charset=utf-8",
            success: getNamesSuccess,
            error: getNamesError
        });
	};
	
	function getNamesSuccess(result){
		var _data = result.d;
		var _json = $.parseJSON(_data);
		var _selectCode = '';
		for(var i=0;i<_json.length;i++){
			_selectCode += '<option value="' + _json[i] + '">' + _json[i] + '</option>';
		}
		$('#selFences').append(_selectCode);
	}
	
	function getNamesError(error){
		return 'error';
	}
	
	this.getKML = function getKMLData(polyName){
		var srvURL = url + 'getKMLByPolygon';
		var _data = 'polyName=' + polyName;
		var _selectCode = '';
		$.ajax({
            type: "GET",
            dataType: "json",
			data: _data,
            url: srvURL,
            contentType: "application/json; charset=utf-8",
            success: getKMLSuccess,
            error: getKMLError
        });
	}
	
	function getKMLSuccess(result){
		var _data = result.d;
		$('#kmlString').empty();
		$('#kmlString').val(_data);
	}
	
	function getKMLError(error){
		alert(error);
	}

	this.getPoly = function getPolyData(polyName){
		var srvURL = url + 'getSpecificGeoFenceDesignerObject';
		var _data = 'mapName=' + polyName;
		var _selectCode = '';
		$.ajax({
            type: "GET",
            dataType: "json",
			data: _data,
            url: srvURL,
            contentType: "application/json; charset=utf-8",
            success: getPolySuccess,
            error: getPolyError
        });
	}
	
	function getPolySuccess(result){
		var _data = result.d;
		var _json = $.parseJSON(_data);
		var _stringJSON = JSON.stringify(_json);
		$('#mapData').val(_stringJSON);
	}
	
	function getPolyError(error){
		alert('Unexpected error');
	}
}

