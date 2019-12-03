import React from 'react';
import { hot } from 'react-hot-loader';

class TodoList extends React.Component {
    render() {
        return (
            <ul>
                {this.props.items.map(item => <li key={item.id} style={{ fontFamily: "verdana", fontSize: 28, color: "red" }}>{item.text}</li>)}
            </ul>
        );
    }
} export default hot(module)(TodoList);