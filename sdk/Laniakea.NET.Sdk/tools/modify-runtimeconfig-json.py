#!/usr/bin/env python3

import sys
import json
from collections import OrderedDict

FRAMEWORK_NAME = 'Laniakea.NET'
FRAMEWORK_VERSION = '0.1.0'

def has_my_lib_framework(frameworks):
    for fw in frameworks:
        if fw['name'] == FRAMEWORK_NAME:
            return True
    return False

if __name__ == '__main__':
    if len(sys.argv) < 3:
        exit(1)

    in_json_path = sys.argv[1]
    out_json_path = sys.argv[2]

    f = open(in_json_path, 'r')
    in_json = f.read()
    f.close()

    in_dict = json.loads(in_json, object_pairs_hook=OrderedDict)

    added_framework = {
        "name": FRAMEWORK_NAME,
        "version": FRAMEWORK_VERSION,
    }

    frameworks = []
    runtime_options = in_dict['runtimeOptions']

    if 'framework' in runtime_options.keys():
        framework = runtime_options['framework']
        frameworks = [
            framework,
            added_framework,
        ]
    elif 'frameworks' in runtime_options.keys():
        if has_my_lib_framework(runtime_options['frameworks']) is False:
            runtime_options['frameworks'].append(added_framework)

    out_dict = OrderedDict({'runtimeOptions': {}})
    for key in runtime_options.keys():
        if key == 'framework':
            out_dict['runtimeOptions']['frameworks'] = frameworks
        else:
            out_dict['runtimeOptions'][key] = runtime_options[key]

    out_json = json.dumps(out_dict, indent=2)

    f = open(out_json_path, 'w')
    f.write(out_json)
    f.close()

