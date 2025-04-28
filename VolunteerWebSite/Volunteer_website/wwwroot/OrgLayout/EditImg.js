
        $(document).ready(function () {
            // Preview Main Image
            $("#mainImageInput").change(function () {
                const file = this.files[0];
                if (file) {
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        $("#mainImageNewPreview").show();
                        $("#mainImageNewPreviewImg").attr("src", e.target.result);
                    };
                    reader.readAsDataURL(file);
                } else {
                    $("#mainImageNewPreview").hide();
                    $("#mainImageNewPreviewImg").attr("src", "");
                }
            });

            // Preview Additional Images
            $("#additionalImagesInput").change(function () {
                $("#additionalImagesNewPreview").show();
                $("#additionalImagesNewPreviewContainer").empty();
                const files = this.files;
                for (let i = 0; i < files.length; i++) {
                    const file = files[i];
                    const reader = new FileReader();
                    reader.onload = function (e) {
                        const img = `<img src="${e.target.result}" alt="New Additional Image" style="max-width: 100px; max-height: 100px; margin-right: 5px;" />`;
                        $("#additionalImagesNewPreviewContainer").append(img);
                    };
                    reader.readAsDataURL(file);
                }
            });

            // Xóa Main Image
            window.removeMainImage = function () {
                $("#mainImagePreview").attr("src", "");
                $("#mainImagePreview").parent().hide();
                $("input[name='existingImagePath']").val("");
                $("#mainImageInput").val(""); // Xóa file đã chọn
                $("#mainImageNewPreview").hide();
            };

            // Xóa Additional Image
            window.removeAdditionalImage = function (button, imgPath) {
                $(button).parent().remove();
                // Cập nhật lại danh sách existingListImg
                const existingImages = $("input[name='existingListImg']").map(function () {
                    return $(this).val();
                }).get().filter(img => img !== imgPath);
                $("input[name='existingListImg']").remove();
                existingImages.forEach(img => {
                    $("#additionalImagesContainer").append(`<input type="hidden" name="existingListImg" value="${img}" />`);
                });
            };

            // Điền thông tin sự kiện (phần code cũ của bạn)
            $("#eventSelector").change(function () {
                var eventId = $(this).val();
                if (eventId) {
                    $.ajax({
                        url: '/Organization/HomeOrg/GetEvent/' + eventId,
                        type: 'GET',
                        success: function (data) {
                            $("#EventId").val(data.eventId);
                            $("#Name").val(data.name);
                            $("#TargetMember").val(data.targetMember);
                            $("#Status").val(data.status.toString());
                            $("#OrgId").val(data.orgId);
                            $("#type_event_name").val(data.type_event_name);
                            $("#DayBegin").val(data.dayBegin ? data.dayBegin.split('T')[0] : '');
                            $("#DayEnd").val(data.dayEnd ? data.dayEnd.split('T')[0] : '');
                            $("#Location").val(data.location);
                            $("#TargetFunds").val(data.targetFunds);
                            $("#Description").val(data.description);

                            // Cập nhật ảnh
                            if (data.imagePath) {
                                $("#mainImagePreview").attr("src", data.imagePath);
                                $("#mainImagePreview").parent().show();
                                $("input[name='existingImagePath']").val(data.imagePath);
                            } else {
                                $("#mainImagePreview").parent().hide();
                                $("input[name='existingImagePath']").val("");
                            }

                            if (data.listImg) {
                                $("#additionalImagesContainer").empty();
                                const images = data.listImg.split(",");
                                images.forEach(img => {
                                    $("#additionalImagesContainer").append(`
                                        <div class="d-flex align-items-center mb-2 mr-2" style="margin-right: 10px;">
                                            <img src="${img}" alt="Additional Image" style="max-width: 100px; max-height: 100px; margin-right: 5px;" class="additional-image" />
                                            <button type="button" class="btn btn-danger btn-sm" onclick="removeAdditionalImage(this, '${img}')">Remove</button>
                                            <input type="hidden" name="existingListImg" value="${img}" />
                                        </div>
                                    `);
                                });
                            } else {
                                $("#additionalImagesContainer").empty();
                            }
                        },
                        error: function () {
                            alert("Error loading event data.");
                        }
                    });
                } else {
                    $("form")[0].reset();
                    $("#mainImagePreview").parent().hide();
                    $("#additionalImagesContainer").empty();
                }
            });

            // Điền thông tin mặc định nếu có sự kiện được chọn ban đầu
            var defaultEventId = $("#EventId").val();
            if (defaultEventId) {
                $("#eventSelector").val(defaultEventId).trigger("change");
            }
        });
