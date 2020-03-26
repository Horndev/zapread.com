//
//

$(document).ready(function () {
    $('#select-icon').selectize({
        labelField: 'name',
        searchField: 'name',
        create: false,
        render: {
            option: function (item, escape) {
                return '<div>' +
                    '<span class="title">' +
                    '<i class="fa fa-2x fa-' + escape(item.name) + '"></i>' + /* + ' ' + escape(item.name) + '</span>'*/
                '</div>';
            },
            item: function (item, escape) {
                return '<div>' +
                    '<span class="title">' +
                    '<i class="fa fa-3x fa-' + escape(item.name) + '"></i>' +
                '</div>';
            }
        }
    });

    $('#multisel').selectize({
        plugins: ['restore_on_backspace', 'remove_button'],
        delimiter: ',',
        persist: false,
        create: function (input) {
            return {
                value: input,
                text: input
            };
        },
        render: {
            option: function (data, escape) {

                return '<div class="option" style="color: #fff;background-color:#1ab394;">' + escape(data.text) + '</div>';
            },
            item: function (data, escape) {
                return '<div class="item" style="color: #fff;background-color:#1ab394;">' + escape(data.text) + '</div>';
            }
        }
    });
});