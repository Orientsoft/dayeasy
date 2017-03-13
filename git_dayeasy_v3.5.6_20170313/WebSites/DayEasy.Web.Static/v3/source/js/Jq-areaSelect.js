/**
 * jQuery区域选择器
 * @author shoy
 * @create 2015/4/9.
 */
(function ($, undefined) {
    $.extend({
        /**
         * 区域判断
         * @param point
         * @param areas
         * @returns {boolean}
         */
        inArea: function (point, areas) {
            var area, getArea, pointArea, itemArea, jw, jh;
            getArea = function (p) {
                return {
                    x0: p.x,
                    x1: p.x + p.width,
                    y0: p.y,
                    y1: p.y + p.height
                };
            };
            pointArea = getArea(point);
            for (var i = 0; i < areas.length; i++) {
                area = areas[i];
                itemArea = getArea(area);
                jw = Math.min(pointArea.x1, itemArea.x1) - Math.max(pointArea.x0, itemArea.x0);
                jh = Math.min(pointArea.y1, itemArea.y1) - Math.max(pointArea.y0, itemArea.y0);
                if (jw > 0 && jh > 0)
                    return true;
            }
            return false;
        }
    });
    var AreaSelectStatus = {CREATE: 'create', MOVE: 'move', RESIZE: 'resize', NEAR: 'near'};
    var Direction = {
        NE: {name: 'NE', x: 1, y: -1, cursor: 'nesw-resize'},
        NW: {name: 'NW', x: -1, y: -1, cursor: 'nwse-resize'},
        SE: {name: 'SE', x: 1, y: 1, cursor: 'nwse-resize'},
        SW: {name: 'SW', x: -1, y: 1, cursor: 'nesw-resize'}
    };
    var DeleteMethod = {CLICK: 'click', DOUBLE_CLICK: 'doubleClick'};

    function AreaSelect($ele, options) {
        this.$ele = $ele;
        this.init();
        this.areas = options.initAreas;
        this.options = options;
        this.status = AreaSelectStatus.CREATE;
        this.dragging = false;
        this.resizeDirection = null;
        this.dragAreaOffset = {};
        this.draw();
    }

    AreaSelect.prototype.get = function () {
        return this.areas;
    };
    AreaSelect.prototype.add = function (area) {
        area && (this.areas = this.areas.concat(area));
        this.draw();
    };
    AreaSelect.prototype.clear = function () {
        this.areas = [];
        this.draw();
    };
    AreaSelect.prototype.set = function (options) {
        this.options = $.extend({}, this.options, options);
    };

    AreaSelect.prototype.bindChangeEvent = function (handle) {
        this.$canvas.on("areasChange", handle[0]);
    };

    AreaSelect.prototype.init = function () {
        var $canvas = $('<canvas/>');
        $canvas.attr('width', this.$ele.width())
            .attr('height', this.$ele.height())
            .offset(this.$ele.position())
            .css({
                position: "absolute",
                zIndex: 200
            })
            .appendTo(this.$ele.parent());
        this.$canvas = $canvas;
        this.g2d = $canvas[0].getContext('2d');
        var as = this;
        var moveDownPoint = {};
        $canvas.mousemove(function (event) {
            var offsetX = get_offset_X(event);
            var offsetY = get_offset_Y(event);
            if (as.dragging) {
                as.onDragging(offsetX, offsetY);
            } else {
                as.onMouseMoving(offsetX, offsetY);
            }
        }).mousedown(function (event) {
            moveDownPoint = {x: get_offset_X(event), y: get_offset_Y(event)};
            as.onDragStart(get_offset_X(event), get_offset_Y(event));
        }).mouseup(function (event) {
            if (get_offset_X(event) == moveDownPoint.x && get_offset_Y(event) == moveDownPoint.y) {
                as.onClick(get_offset_X(event), get_offset_Y(event));
            }
            as.onDragStop();
        }).dblclick(function (event) {
            as.onDoubleClick(get_offset_X(event), get_offset_Y(event));
        });
    };

    AreaSelect.prototype.onDragStart = function (x, y) {
        if (!this.options.drag)
            return;
        this.dragging = true;
        switch (this.status) {
            case AreaSelectStatus.RESIZE:
                !this.currentArea || setAreaDirection(this.currentArea, this.resizeDirection);
                break;
            case AreaSelectStatus.MOVE:
                this.dragAreaOffset = {x: this.currentArea.x - x, y: this.currentArea.y - y};
                break;
            case AreaSelectStatus.CREATE:
                if (!this.options.create)
                    return;
                var newArea = {x: x, y: y, width: 0, height: 0};
                this.areas.push(newArea);
                this.currentArea = newArea;
                this.status = AreaSelectStatus.RESIZE;
                break;
        }
    };

    AreaSelect.prototype.onDragStop = function () {
        if (!this.options.drag)
            return;
        this.dragging = false;
        switch (this.status) {
            case AreaSelectStatus.RESIZE:
                if (this.currentArea != undefined) {
                    if (this.currentArea.width == 0 && this.currentArea.height == 0) {
                        this.deleteArea(this.currentArea);
                        this.currentArea = undefined;
                        this.status = AreaSelectStatus.CREATE;
                    } else {
                        setAreaDirection(this.currentArea, Direction.SE);
                        this.triggerChange();
                    }
                }
                break;
            case AreaSelectStatus.MOVE:
                this.triggerChange();
                break;
        }
    };

    AreaSelect.prototype.onMouseMoving = function (x, y) {
        var area = this.getArea(x, y, this.options.padding);
        var $canvas = this.$canvas;
        if (area != undefined) {
            this.currentArea = area;
            var nearDrag = false;
            var dragDirection = null;
            var dragPoints = getPositionPoints(area);
            for (var d in dragPoints) {
                if (near({x: x, y: y}, dragPoints[d], this.options.padding)) {
                    nearDrag = true;
                    dragDirection = Direction[d];
                    break;
                }
            }
            if (nearDrag) {
                $canvas.css({cursor: dragDirection.cursor});
                this.status = AreaSelectStatus.RESIZE;
                this.resizeDirection = dragDirection;
            }
            else if (this.getArea(x, y, -this.options.padding) != undefined) {
                $canvas.css({cursor: 'move'});
                this.status = AreaSelectStatus.MOVE;
            } else {
                $canvas.css({cursor: 'auto'});
                this.status = AreaSelectStatus.NEAR;
            }
        } else {
            this.currentArea = undefined;
            $canvas.css({cursor: 'auto'});
            this.status = AreaSelectStatus.CREATE;
        }
        this.draw();
    };

    AreaSelect.prototype.onDragging = function (x, y) {
        if (!this.options.drag)
            return;
        var area = this.currentArea;
        switch (this.status) {
            case AreaSelectStatus.RESIZE:
                area.width = x - area.x;
                area.height = y - area.y;
                break;
            case AreaSelectStatus.MOVE:
                area.x = (x + this.dragAreaOffset.x);
                area.y = (y + this.dragAreaOffset.y);
                break;
            case AreaSelectStatus.CREATE:
                break;
        }
        this.draw();
    };


    AreaSelect.prototype.onDoubleClick = function (x, y) {
        var area = this.getArea(x, y, this.options.padding);
        if (area != undefined && this.options.deleteMethod == DeleteMethod.DOUBLE_CLICK) {
            this.deleteArea(area);
            this.draw();
        }
    };

    AreaSelect.prototype.onClick = function (x, y) {
        var area = this.getArea(x, y, this.options.padding);
        if (area != undefined && this.options.deleteMethod == DeleteMethod.CLICK) {
            this.deleteArea(area);
            this.draw();
        }
    };

    AreaSelect.prototype.draw = function () {
        var g2d = this.g2d, area, index;
        /* clear canvas */
        g2d.clearRect(0, 0, this.$canvas[0].width, this.$canvas[0].height);
        /* draw areas */
        g2d.strokeStyle = this.options.area.strokeStyle;
        g2d.lineWidth = this.options.area.lineWidth;
        for (index in this.areas) {
            area = this.areas[index];
            if (area.num) {
                this.g2d.fillStyle = this.options.point.fillStyle;
                var width = Math.max(area.num.length * 7.5, 13) + 12;
                this.g2d.fillRect(area.x, area.y, width, 25);
                this.g2d.fillStyle = '#FFFFFF';
                this.g2d.font = "normal 14px arial";
                this.g2d.fillText(area.num, area.x + 6, area.y + 19);
            }
            this.g2d.strokeRect(area.x, area.y, area.width, area.height);
        }
        /* draw current area */
        area = this.currentArea;
        g2d.fillStyle = this.options.point.fillStyle;
        if (area != undefined) {
            //g2d.strokeStyle = "rgba(255,0,0,0.2)";
            //g2d.fillRect(0, 0, 30, 30);
            //g2d.strokeRect(0, 0, 30, 30);
            //g2d.font = "normal 16px arial";
            //g2d.fillStyle = "#FFFFFF";
            //g2d.fillText("12", area.x, area.y + 20);
            //g2d.fillStyle = this.options.point.fillStyle;
            var positionPoints = getPositionPoints(area);
            /* draw position point */
            for (index in positionPoints) {
                var point = positionPoints[index],
                    size = this.options.point.size,
                    type = this.options.point.type;
                g2d.beginPath();
                switch (type) {
                    case "rect":
                        var _x = point.x - size, _y = point.y - size, _w = size * 2, _h = size * 2, _r = 0;
                        g2d.moveTo(_x + _r, _y);
                        g2d.arcTo(_x + _w, _y, _x + _w, _y + _h, _r);
                        g2d.arcTo(_x + _w, _y + _h, _x, _y + _h, _r);
                        g2d.arcTo(_x, _y + _h, _x, _y, _r);
                        g2d.arcTo(_x, _y, _x + _w, _y, _r);
                        break;
                    default :
                        g2d.arc(point.x, point.y, this.options.point.size, 0, Math.PI * 2, true);
                        break;
                }
                g2d.closePath();
                g2d.fill();
            }
        }
    };

    AreaSelect.prototype.deleteArea = function (area) {
        var areas = this.areas;
        var index = areas.indexOf(area);
        if (index >= 0) {
            areas.splice(areas.indexOf(area), 1);
            this.currentArea = undefined;
            this.triggerChange();
            this.status = AreaSelectStatus.CREATE;
        }
    };

    AreaSelect.prototype.getArea = function (x, y, padding) {
        padding = padding === undefined ? 0 : padding;
        for (var index in this.areas) {
            var area = this.areas[index];
            var abs = Math.abs;
            var x1 = area.x;
            var x2 = area.x + area.width;
            var y1 = area.y;
            var y2 = area.y + area.height;
            if (padding >= 0 && abs(x1 - x) + abs(x2 - x) - abs(area.width) <= padding * 2
                && abs(y1 - y) + abs(y2 - y) - abs(area.height) <= padding * 2) {
                return area;
            }
            if (padding < 0
                && abs(x1 - x) + abs(x2 - x) - abs(area.width) == 0
                && abs(y1 - y) + abs(y2 - y) - abs(area.height) == 0
                && abs(abs(x1 - x) - abs(x2 - x)) <= abs(area.width) + 2 * padding
                && abs(abs(y1 - y) - abs(y2 - y)) <= abs(area.height) + 2 * padding) {
                return area;
            }
        }
        return undefined;
    };

    AreaSelect.prototype.triggerChange = function () {
        this.$canvas.trigger("areasChange", {areas: this.areas});
    };

    var getPositionPoints = function (area) {
        var points = {};
        for (var d in Direction) {
            points[d] = {
                x: area.x + area.width * (Direction[d].x + 1) / 2,
                y: area.y + area.height * (Direction[d].y + 1) / 2
            };
        }
        return points;
    };
    var setAreaDirection = function (area, direction) {
        if (area != undefined && direction != undefined) {
            var x1 = area.x;
            var x2 = area.x + area.width;
            var y1 = area.y;
            var y2 = area.y + area.height;
            var width = Math.abs(area.width);
            var height = Math.abs(area.height);
            var minOrMax = {'1': Math.min, '-1': Math.max};
            area.x = minOrMax[direction.x](x1, x2);
            area.y = minOrMax[direction.y](y1, y2);
            area.width = direction.x * width;
            area.height = direction.y * height;
        }
    };

    var near = function (point1, point2, s) {
        return Math.pow(point1.x - point2.x, 2) + Math.pow(point1.y - point2.y, 2) <= Math.pow(s, 2);
    };

    var contains = function (point1, point2) {

    };

    var get_offset_X = function (event) {
        return event.offsetX ? event.offsetX : event.originalEvent.layerX;
    };

    var get_offset_Y = function (event) {
        return event.offsetY ? event.offsetY : event.originalEvent.layerY;
    };


    $.fn.areaSelect = function (method) {
        var as;
        var defaultOptions = {
            initAreas: [],
            deleteMethod: 'click',//or doubleClick
            padding: 3,
            area: {strokeStyle: 'red', lineWidth: 2},
            point: {size: 3, fillStyle: 'black', type: 'circle'},
            create: true,
            canRepeat: true,
            drag: true
        };
        as = this.data('AreaSelect');
        if (as == undefined && (method === undefined || $.isPlainObject(method))) {
            var options = $.extend({}, defaultOptions, method);
            as = new AreaSelect(this, options);
            this.data('AreaSelect', as);
        } else {
            if (as === undefined) {
                console.error('pls invoke areaSelect() on this element first!');
            } else if (as[method] != undefined) {
                return as[method](Array.prototype.slice.call(arguments, 1));
            } else {
                console.error('no function ' + method);
            }
        }
    };
})(jQuery);

