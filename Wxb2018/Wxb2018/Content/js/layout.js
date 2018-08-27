$(function () {

    var _isLogin = $('input[name="isLogin"]').val(),
        _loginHeadPartialUrl = $('input[name="loginHeadPartialUrl"]').val(),
        _accountPartialUrl = $('input[name="accountPartialUrl"]').val();

    if (_isLogin) {

        //获取登录后头部html
        $.get(_loginHeadPartialUrl + '?t' + new Date().getTime(), function (backHtml) {

            if (backHtml.indexOf("div") != -1) {
                $(".topcon div.login").html(backHtml);
            }
        });
    } else {

        $("#accountDiv").load(_accountPartialUrl + '?t=' + Math.random(), function () {

            //点击'退出'后调用
            window.loadAccountDivDone && loadAccountDivDone();
        });
    }

    //获取应急状态
    getResponseStatus();

    //获取当前登录者值班信息
    getUserDutyStatus();

    //记录
    $('.record').hover(function () {
        $('.record-type').show();
    }, function () {
        $('.record-type').hide();
    });

    //上班
    $('.icon-punchin').on('click', function (e) {

        var addDutyLogUrl = $('input[name="addDutyLogUrl"]').val();
        $.post(addDutyLogUrl, {}, function (data) {

            if (data.Code > 0) {
                getUserDutyStatus();
            } else {
                alert(data.Msg);
            }
        });
    });

    //下班
    $('.icon-punchout').on('click', function (e) {

        var updateDutyStatusUrl = $('input[name="updateDutyStatusUrl"]').val();
        $.post(updateDutyStatusUrl, { ID: $('.punch').data('dutyid') }, function (data) {

            if (data.Code > 0) {
                getUserDutyStatus();
            } else {
                alert(data.Msg);
            }
        });
    });

    //点击'响应状态'
    $('.icon-gear').on('click', function (e) {

        var editResponseUrl = $('input[name="editResponseUrl"]').val();
        var options = {
            title: false,
            type: 1,
            skin: 'response-show',
            area: ['360px', '200px'],
            closeBtn: false,
            shade: 0.3,
            offset:['100px'],
            move: false,
            content: $('#responseDiv'),
            success: function (layero, index) {

                //绑定关闭按钮
                $('#responseDiv').find('.cancelResponse').data('rpshowindex', index);
                $(document.body).attr('data-opening', '1');
                //加载html
                $.get(editResponseUrl + '?ID=' + $('.response-val').data('id'), function (backHtml) {

                    $('#responseDiv .rp-content').html(backHtml);
                });
            }
        };

        layer.open(options);
    });

    //关闭'响应状态'dialog
    $('.cancelResponse').on('click', function () {

        var rpshowIndex = $(this).data('rpshowindex')
        layer.close(rpshowIndex);
        $(document.body).attr('data-opening', '0');
    });

    // '保存'关闭弹窗
    $('.saveResponse').on('click', function () {
     
        var saveResponseUrl = $('input[name="saveResponseUrl"]').val(),
            rpParams = {
                ID: $('.response-val').data('id'),
                Status: $('input[name="status"]:checked').val(),
                StartTime: $('.startDate').val(),
                EndTime: $('.endDate').val()
            };

        $.post(saveResponseUrl, rpParams, function (data) {

            if (data.Code == 200) {

                $('.response-val').data('id', data.Data.ID);
                $('.response-val').data('status', data.Data.Status);
                $('.response-val').text(data.Data.StatusText);
            } else {
                alert(data.Msg);
            }
        });

        //关闭dialog
        $('.cancelResponse').trigger('click');
    });

    //获取当前登录者值班信息
    function getUserDutyStatus() {

        $.ajax({
            url: $('input[name="getUserDutyLogUrl"]').val(),
            dataType: 'json',
            type: 'get',
            success: function (data) {

                var punchIn = $('.icon-punchin,.punchin-text');
                var punchOut = $('.icon-punchout,.punchout-text');
                var punchNames = $('.onduty-text,.onduty-names');

                if (data.Code < 0) {
                    punchOut.hide();
                    punchNames.hide();
                    punchIn.show();
                    return;
                }

                var result = data.Data,
                    ondutyNames = $('.onduty-names'),
                    sumNames = '';

                if (data.Code == 200) {

                    punchIn.hide();
                    punchOut.show();
                    punchNames.show();
                    ondutyNames.text(result.CurrentDutyUser);
                    $('.punch').data('dutyid', result.CurrentDutyID);

                    if (result.OtherDutyUser) {
                        for (var i = 0; i < result.OtherDutyUser.length; i++) {

                            sumNames += ',' + result.OtherDutyUser[i].UserName;
                        }

                        sumNames = ondutyNames.text() + sumNames;
                        ondutyNames.text(sumNames);
                    }

                } else if (data.Code == 201) {

                    punchIn.show();
                    punchOut.hide();
                    punchNames.show();
                    for (var i = 0; i < result.OtherDutyUser.length; i++) {

                        sumNames += ',' + result.OtherDutyUser[i].UserName;
                    }

                    ondutyNames.text(sumNames.substr(1));
                }

            }
        });
    }

    //首页加载获取响应状态
    function getResponseStatus() {

        $.ajax({
            url: $('input[name="getResponseStatusUrl"]').val(),
            dataType: 'json',
            type: 'get',
            success: function (data) {

                var response = $('.response-val');
                if (data.Code > 0) {
                    response.text(data.Data.StatusText);
                    response.attr('data-status', data.Data.Status);
                    response.attr('data-id', data.Data.ID);
                } else {
                    response.text('应急');
                    response.attr('data-status', 1);
                }
            }
        });
    }

});
