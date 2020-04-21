/**
 * Get the account balance and update all UI elements with new value
 **/

export async function updateuserbalance() {
    const response = await fetch('/Account/Balance/');
    const json = await response.json();
    var elements = document.querySelectorAll(".userBalanceValue");
    Array.prototype.forEach.call(elements, function (el, _i) {
        el.innerHTML = json.balance;
    });

    // Old jquery way
    //$.get("/Account/Balance", function (data, status) {
    //    $(".userBalanceValue").each(function (i, e) {
    //        $(e).html(data.balance);
    //    });
    //});
}