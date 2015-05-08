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
        editorTypes = [
            'choice',
            'textfield',
            'spinner',
            'date',
            'choice',
            'choice',
            'choice',
            'textfield',
            'textfield'
        ],
        seed = 1,
        rnd = function() {
            var x = Math.sin(seed++)*10000,
                r = x - Math.floor(x);
            return r;
        },
        numRows = 100000,
        firstNames = ['Olivia', 'Sophia', 'Ava', 'Isabella', 'Boy', 'Liam', 'Noah', 'Ethan', 'Mason', 'Logan', 'Moe', 'Larry', 'Curly', 'Shemp', 'Groucho', 'Harpo', 'Chico', 'Zeppo', 'Stanley', 'Hardy'],
        lastNames = ['Wirts', 'Oneil', 'Smith', 'Barbarosa', 'Soprano', 'Gotti', 'Columbo', 'Luciano', 'Doerre', 'DePena'],
        months = ['01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12'],
        days = ['01', '02', '03', '04', '05', '06', '07', '08', '09', '10', '11', '12', '13', '14', '15', '16', '17', '18', '19', '20', '21', '22', '23', '24', '25', '26', '27', '28', '29', '30'],
        states = ['Alabama','Alaska','Arizona','Arkansas','California','Colorado','Connecticut','Delaware','Florida','Georgia','Hawaii','Idaho','Illinois','Indiana','Iowa','Kansas','Kentucky','Louisiana','Maine','Maryland','Massachusetts','Michigan','Minnesota','Mississippi','Missouri','Montana','Nebraska','Nevada','New Hampshire','New Jersey','New Mexico','New York','North Carolina','North Dakota','Ohio','Oklahoma','Oregon','Pennsylvania','Rhode Island','South Carolina','South Dakota','Tennessee','Texas','Utah','Vermont','Virginia','Washington','West Virginia','Wisconsin','Wyoming'],
        randomFunc = Math.random,
        randomPerson = function() {
            var firstName = Math.round((firstNames.length - 1) * randomFunc()),
                lastName = Math.round((lastNames.length - 1) * randomFunc()),
                pets = Math.round(10 * randomFunc()),
                birthyear = 1900 + Math.round(randomFunc() * 114),
                birthmonth = Math.round(randomFunc() * 11),
                birthday = Math.round(randomFunc() * 29),
                birthstate = Math.round(randomFunc() * 49),
                residencestate = Math.round(randomFunc() * 49),
                travel = randomFunc() * 1000,
                income = randomFunc() * 100000,
                employed = Math.round(randomFunc()),
                person = {
                    Last: lastNames[lastName],
                    First: firstNames[firstName],
                    Pets: pets,
                    BirthDate: birthyear + '-' + months[birthmonth] + '-' + days[birthday],
                    BirthState: states[birthstate],
                    ResidenceState: states[residencestate],
                    Wmployed: employed === 1,
                    Income: income,
                    Travel: travel
                };
            return person;
        },
        data = [];

        for (var i = 0; i < numRows; i ++) {
            data.push(randomPerson());
        }
    document.addEventListener('polymer-ready', function() {

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

        fin.desktop.main(function (){
            fin.desktop.InterApplicationBus.subscribe("*",
                "more-data",
                function (message, uuid) {
                    jsonModel.setData(message.data);
            });
        });

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