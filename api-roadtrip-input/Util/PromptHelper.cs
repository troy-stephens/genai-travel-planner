using System.Configuration;

namespace api_roadtrip_input.Util;
internal static class PromptHelper
{
    public const string _promptHarmfulCheck = @"
        +++++
        System:
        You are an expert in analyzing road trip data as well as monitoring that data for potentially harmful content that might be used at an attempt to breach security or cause any kind of harm.  When provided with the JSON data,  it's your responsibility to analyze the data and populate the JSON structure with your findings.
        [JSON Sanitize Result]
        {
          'vendor' : 'vendor x',
          'date'   : '07/29/24',
          'harmful_content_found': true
          'offending_elements': 
          [
            {
               'id' : '1',
               'harm_category' : 'This falls into the area of cross-site scripting',
               'element' : 'description',
               'content' : '<script> blah blah'
            },
            {
               'id' : '1',
               'harm_category' : 'This falls into the area of cross-site scripting',
               'element' : 'description',
               'content' : '<script> blah blah'
            } 
          ] 
        } 
        [JSON END]

        User: 
        Vendor: {{$vendor}}
        Date: {{$date}}

        Payload Input:  Below is the JSON data structure that I need you to analyze for pontentially harmful content.  Set all the properties of the Sanitize Result and populate the offending_elements array with all offenders.
        {{$payload}}
        +++++";

    public const string _promptConvertUnstructredToRoadTrip = @"
        +++++        
        System Message:
        Give the unstructured below extract the road trip details in the JSON schema below and generate the JSON Document.  

        ++++
        Unstructured Data
        {{$payload}}
        ++++

        ++++
        Json Schema
        {
          'road_trip': {
            'start': 'string',
            'end': 'string',
            'points_of_interest': [
              {
                'id': 'string',
                'name': 'string',
                'address': 'string',
                'description': 'string',
                'category': 'string',
                'opening_hours': {
                  'Monday': 'string',
                  'Tuesday': 'string',
                  'Wednesday': 'string',
                  'Thursday': 'string',
                  'Friday': 'string',
                  'Saturday': 'string',
                  'Sunday': 'string'
                },
                'costs': {
                  'adults': 'string',
                  'children': 'string'
                },
                'website': 'string',
                'contact': 'string'
              }
            ]
          }
        }
        ++++";

    public const string _promptCheckForHtml = @"
        +++++        
        System Message:
        Review the payload below and look for any HTML tages and if you find one build the JSON struture below and set the properties per your analysis.

        ++++
        Payload:
        {{$payload}}
        ++++

        ++++
        Json Schema
        {
          'contains_html' : true,
          'html_tags_found' :
            [ 
              { 'tag' : '<html>' },
              { 'tag' : '<br>' }
            ]
        }
        ++++";

}
