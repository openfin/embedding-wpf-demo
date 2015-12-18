//setup random data for the JSON tab example
(function(){
   var headers = [
            'last',
            'first',
            'pets',
            'birth date',
            'birth state',
            'residence state',
            'employed',
            'income',
            'travel'
        ],
        fields = [
            'Last',
            'First',
            'Pets',
            'BirthDate',
            'BirthState',
            'ResidenceState',
            'Employed',
            'Income',
            'Travel'
        ],
        data = [];
    document.addEventListener('polymer-ready', function() {
        fin.desktop.main(function (){
            fin.desktop.InterApplicationBus.subscribe("*",
                "user-data",
                function (message, uuid) {
                    jsonModel.setData(message.data);
            });
        });
        //get ahold of our json grid example
        var jsonGrid = document.querySelector('#json-example');

        //get it's table model
        var jsonModel = jsonGrid.getBehavior();

        //get the cell cellProvider for altering cell renderers
        var cellProvider = jsonModel.getCellProvider();

        //set the header labels
        jsonModel.setHeaders(headers);

        //set the fields found on the row objects
        jsonModel.setFields(fields);

        //set the actual json row objects
        jsonModel.setData(data);
        window.assignData = function (data) {
            jsonModel.setData(data);
        };

        //all formatting and rendering per cell can be overridden in here
        cellProvider.getCell = function(config) {
            var renderer = cellProvider.cellCache.simpleCellRenderer;
            config.halign = 'left';
            var x = config.x;
            if (x === 2) {
                config.halign = 'center';
            } else if (x === 3) {
                config.halign = 'center';
            } else if (x === 6) {
                config.halign = 'center';
            } else if (x === 7) {
                var travel = 60 + Math.round(config.value*150/100000);
                var bcolor = travel.toString(16);
                config.halign = 'right';
                config.bgColor = '#00' + bcolor + '00';
                config.fgColor = '#FFFFFF';
                config.value = accounting.formatMoney(config.value);
            } else if (x === 8) {
                var travel = 105 + Math.round(config.value*150/1000);
                var bcolor = travel.toString(16);
                config.halign = 'right';
                config.bgColor = '#' + bcolor+ '0000';
                config.fgColor = '#FFFFFF';
                config.value = accounting.formatMoney(config.value, "€", 2, ".", ",");
            }

            renderer.config = config;
            return renderer;
        };
    });
})();