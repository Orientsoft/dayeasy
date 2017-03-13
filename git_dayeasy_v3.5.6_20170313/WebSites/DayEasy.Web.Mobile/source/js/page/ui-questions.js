/**
 * Created by Boz 2016/6/21.
 */


var QuestionFunc= function (question){
    console.log(question);
    var html;
    html = '';
    html += '<div class="q-item">';
    html += '<div class="q-title">' + question.sort + '.' + '</div>';
    html += '<div class="q-body">'
    html += '<div>' + question.body + '</div>';

    if(question.details.length > 0){
        for (var i = 0; i < question.details.length; i++) {
            html+='<div>'+question.details[i].body+'</div>';
            if(question.details[i].isObjective){
                for (var j = 0; j < question.details[i].answers.length;j++) {
                    html+='<div>'+question.details[i].answers[j].body+'</div>';
                }
            }
        }
    }else if(question.isObjective){
        for (var k = 0; k < question.answers.length; k++) {
            html+='<div>'+question.answers[k].body+'</div>';
        }
    }
    html += '</div>';
    html += '</div>';
    return html;
}


